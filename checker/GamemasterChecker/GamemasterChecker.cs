using EnoCore.Models;
using EnoCore.Models.Database;
using EnoCore.Models.Json;
using EnoCore.Utils;
using Gamemaster.Models.View;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MongoDB.Driver.Core.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GamemasterChecker
{
    public class GamemasterChecker : IChecker
    {
        private readonly IHttpClientFactory HttpFactory;
        private readonly ILogger Logger;
        private readonly GamemasterDatabase Db;




        public GamemasterChecker(IHttpClientFactory httpFactory, GamemasterDatabase db, ILogger<GamemasterChecker> logger)
        {
            HttpFactory = httpFactory;
            Logger = logger;
            Db = db;
        }

        public async Task<CheckerResult> HandlePutFlag(CheckerTaskMessage task, CancellationToken token)
        {
            if (task.Flag == null)
                throw new InvalidOperationException("Flag must not be null in putflag");
            if ((task.FlagIndex % 2) == 0)
            {
                return await PutFlagToSession(task, token);
            }
            else
            {
                return await PutFlagToToken(task, token);
            }
        }
        public async Task<CheckerResult> HandleGetFlag(CheckerTaskMessage task, CancellationToken token)
        {
            if (task.Flag == null)
                throw new InvalidOperationException("Flag must not be null in getflag");
            if ((task.FlagIndex % 2) == 0)
            {
                return await GetFlagFromSession(task, token);
            }
            else
            {
                return await GetFlagFromToken(task, token);
            }
        }

        public async Task<CheckerResult> HandleGetNoise(CheckerTaskMessage task, CancellationToken token)
        {
            return CheckerResult.Ok;
        }

        public async Task<CheckerResult> HandlePutNoise(CheckerTaskMessage task, CancellationToken token)
        {
            return CheckerResult.Ok;
        }

        public async Task<CheckerResult> HandleHavok(CheckerTaskMessage task, CancellationToken token)
        {
            return CheckerResult.Ok;
        }

        private GamemasterUser CreateUser(long roundId, long teamId, string flag)
        {
            var u = new GamemasterUser()
            {
                RoundId = roundId,
                TeamId = teamId,
                Email = "Test",
                Password = "ultrasecurepw",
                Flag = flag,
                Username = $"Herbert{Environment.TickCount}|{Utils.Random.Next()}"
            };
            return FakeUsers.getFakeUser(u);
        }
        private async Task<CheckerResult> PutFlagToSession(CheckerTaskMessage task, CancellationToken token)
        {
            // Get random user subset from last round
            var users = new List<GamemasterUser>();
            await Db.GetUsersAsync(task.RoundId - 1, task.TeamId, token);
            Logger.LogDebug($"Users from Last Round: {users.Count()}");
            users = users.Where(u => Utils.Random.Next() % 2 == 0).ToList();
            Logger.LogDebug($"Users after pruning: {users.Count()}");
            // Register a new master
            var master = CreateUser(task.RoundId, task.TeamId, task.Flag!);
            //master.Username = "Herbert" + task.Flag + Environment.TickCount.ToString();
            using var masterClient = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, master, Logger);
            bool result;
            try
            {
                result = await masterClient.RegisterAsync(token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToFancyString());
                return CheckerResult.Offline;
            }
            if (!result)
                return CheckerResult.Mumble;

            // Create a new session
            SessionView? session;
            try
            {
                session = await masterClient.CreateSessionAsync("name", task.Flag!, "password", token);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToFancyString());
                return CheckerResult.Offline;
            }
            if (session == null || session.Id == 0)
                return CheckerResult.Mumble;

            // Create new users
            var usersToCreate = Utils.Random.Next(4, 8);
            var newUsers = usersToCreate - users.Count;
            Logger.LogInformation($"Target User Count: {usersToCreate}");
            var registerTasks = new List<Task<bool>>();
            for (int i = 0; i < newUsers; i++)
            {
                var user = CreateUser(task.RoundId, task.TeamId, task.Flag!);
                users.Add(user);
                registerTasks.Add(Task.Run(async () =>
                {
                    using var client = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, user, Logger);
                    return await client.RegisterAsync(token);
                }));
            }
            try
            {
                foreach (var registerTask in registerTasks)
                {
                    if (!await registerTask)
                        return CheckerResult.Mumble;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToFancyString());
                return CheckerResult.Offline;
            }

            // Have master add all users to session

            Logger.LogInformation($"Adding {users.Count} users to session");
            var addSessionTasks = new List<Task<bool>>();
            foreach (var user in users)
            {
                addSessionTasks.Add(masterClient.AddUserToSessionAsync(session.Id, user.Username, token));
            }
            try
            {
                foreach (var addSessionTask in addSessionTasks)
                {
                    if (!await addSessionTask)
                        return CheckerResult.Mumble;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToFancyString());
                return CheckerResult.Offline;
            }
            // Save all users to db
            Logger.LogInformation($"Saving {users.Count} users to db");
            foreach (var user in users)
            {
                user.SessionId = session.Id;
                Logger.LogInformation($"roundId is:{user.RoundId}, tId is:{user.TeamId}");
                //await Db.AddUserAsync(user, token);
            }
            await Db.InsertUsersAsync(users, token);
            Logger.LogInformation("Users added to Db");
            return CheckerResult.Ok;
        }
        private async Task<CheckerResult> PutFlagToToken(CheckerTaskMessage task, CancellationToken token)
        {
            var smaster = CreateUser(task.RoundId, task.TeamId, task.Flag!);
            //smaster.Username = "Herbert" + task.Flag + Environment.TickCount.ToString();
            using var masterClient = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, smaster, Logger);
            bool result;
            try
            {
                result = await masterClient.RegisterAsync(token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToFancyString());
                return CheckerResult.Offline;
            }
            if (!result)
                return CheckerResult.Mumble;

            // Create a new session
            SessionView? session;
            try
            {
                session = await masterClient.CreateSessionAsync("name", "No 🏳️‍🌈 here, go away...", "password", token);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToFancyString());
                return CheckerResult.Offline;
            }
            if (session == null || session.Id == 0)
                return CheckerResult.Mumble;
            byte[] imgdata = new byte[64];
            Utils.Random.NextBytes(imgdata);
            var UUID = await masterClient.AddTokenAsync("name", task.Flag!, true, imgdata, token);
            if (UUID==null  || !isValid(UUID)) return CheckerResult.Mumble;
            await Db.AddTokenUUIDAsync(task.Flag!, UUID, token);
            await Db.AddUserAsync(smaster, token);
            return CheckerResult.Ok;
        }
        private async Task<CheckerResult> GetFlagFromSession(CheckerTaskMessage task, CancellationToken token)
        {
            Logger.LogInformation($"Fetching Users with relrID{task.RelatedRoundId}, tIdis:{task.TeamId}");
            var users = await Db.GetUsersAsync(task.Flag, token);
            Logger.LogInformation($"found {users.Count}");                       //################################## Mumble?
            if (users.Count <= 0) return CheckerResult.Mumble;
            using var client = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, users[0], Logger);
            try
            {
                if (!(await client.LoginAsync(token))) return CheckerResult.Mumble;
            }
            catch (Exception)
            {
                return CheckerResult.Offline;
            }
            ExtendedSessionView session;
            try
            {
                session = await client.FetchSessionAsync(users[0].SessionId, token);
            }
            catch (Exception)
            {
                return CheckerResult.Offline;
            }
            Logger.LogInformation($"Retrieved Flag is {session.Notes}");
            Logger.LogInformation($"Requested Flag is {task.Flag}");
            if (session.Notes.Equals(task.Flag))
            {
                Logger.LogInformation("Flags are Equal");
                return CheckerResult.Ok;
            }
            Logger.LogInformation("Flags are not Equal");
            return CheckerResult.Mumble;
        }
        private async Task<CheckerResult> GetFlagFromToken(CheckerTaskMessage task, CancellationToken token)
        {
            Logger.LogInformation("Fetching Token From Db");
            string gtoken = await Db.GetTokenUUIDAsync(task.Flag!, token);
            if (gtoken == "")
            {
                Logger.LogInformation("No Token Found in Db");
                return CheckerResult.Mumble;
            }
            var smaster = await Db.GetUsersAsync(task.Flag!, token);
            if (smaster == null || smaster.Count != 1)
            {
                Logger.LogInformation($"Master User for the Token not found in Db, or multiple found for the flag: Count:{((smaster!=null) ? smaster.Count:-1)}");
                return CheckerResult.Mumble;
            }
            var mclient = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, smaster[0], Logger);
            TokenStrippedView retrievedToken;
            try
            {
                Logger.LogInformation("trying to retrieve Token");
                retrievedToken = await mclient.CheckTokenAsync(gtoken, token);
                Logger.LogInformation($"Retrieved Token: {retrievedToken}");
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToFancyString());
                return CheckerResult.Offline;
            }
            
            if (retrievedToken.Description.Equals(task.Flag)) return CheckerResult.Ok;
            else return CheckerResult.Mumble;
            
            return CheckerResult.Ok;
        }
        private bool isValid (string UUID)
        {
            if (UUID.Length != 128) return false;
            return true;
        }
    }
}
