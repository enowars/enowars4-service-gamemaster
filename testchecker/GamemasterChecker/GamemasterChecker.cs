using EnoCore.Models;
using EnoCore.Models.Database;
using EnoCore.Models.Json;
using Gamemaster.Models.View;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
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

            // Get random user subset from last round
            var users = new List<GamemasterUser>(); // await Db.GetUsersAsync(task.Round - 1, task.TeamId, token);
            Logger.LogDebug($"Users from Last Round: {users.Count()}");
            users = users.Where(u => Utils.Random.Next() % 2 == 0).ToList();
            Logger.LogDebug($"Users after pruning: {users.Count()}");
            // Register a new master
            var master = CreateUser(task.RoundId, task.TeamId);
            master.Username = "Herbert" + task.Flag + Environment.TickCount.ToString();
            using var masterClient = new GamemasterClient(HttpFactory.CreateClient("default"), task.Address, master, Logger);
            bool result;
            try
            {
                result = await masterClient.RegisterAsync(token).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return CheckerResult.Offline;
            }
            if (!result)
                return CheckerResult.Mumble;

            // Create a new session
            SessionView? session;
            try
            {
                session = await masterClient.CreateSessionAsync("name", task.Flag, "password", token);
            }
            catch (Exception)
            {
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
                var user = CreateUser(task.RoundId, task.TeamId);
                users.Add(user);
                registerTasks.Add(Task.Run(async () =>
                {
                    using var client = new GamemasterClient(HttpFactory.CreateClient("default"), task.Address, user, Logger);
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
            catch (Exception)
            {
                return CheckerResult.Offline;
            }

            // Have master add all users to session

            /*
            foreach (var user in users)
            {
                var success = await masterClient.AddUserToSession(session.Id, user.Username, token);
                if (!success)
                    return CheckerResult.Mumble;
            }

    */
            Logger.LogInformation($"Adding {users.Count} users to session");
            var addSessionTasks = new List<Task<bool>>();
            foreach (var user in users)
            {
                addSessionTasks.Add(masterClient.AddUserToSession(session.Id, user.Username, token));
            }
            try
            {
                foreach (var addSessionTask in addSessionTasks)
                {
                    if (!await addSessionTask)
                        return CheckerResult.Mumble;
                }
            }
            catch (Exception)
            {
                return CheckerResult.Offline;
            }
            // Save all users to db
            Logger.LogInformation($"Saving {users.Count} users to db");
            foreach (var user in users)
            {
                Logger.LogInformation($"roundIdis:{user.RoundId}, tIdis:{user.TeamId}");
                //await Db.AddUserAsync(user, token);
            } 
            await Db.InsertUsersAsync(users, token);
            return CheckerResult.Ok;
        }
        public async Task<CheckerResult> HandleGetFlag(CheckerTaskMessage task, CancellationToken token)
        {
            Logger.LogInformation($"Fetching Users with relrID{task.RelatedRoundId}, tIdis:{task.TeamId}");
            var users = await Db.GetUsersAsync(task.RelatedRoundId, task.TeamId, token);
            Logger.LogInformation($"found {users.Count}");
            if (users.Count <= 0) return CheckerResult.Mumble;
            using var client = new GamemasterClient(HttpFactory.CreateClient("default"), task.Address, users[0], Logger);
            await client.LoginAsync(token);
            await Task.Delay(1000);
            return CheckerResult.Ok;
        }

        public async Task<CheckerResult> HandleGetNoise(CheckerTaskMessage task, CancellationToken token)
        {
            await Task.Delay(1000);
            throw new NotImplementedException();
        }

        public async Task<CheckerResult> HandlePutNoise(CheckerTaskMessage task, CancellationToken token)
        {
            await Task.Delay(1000);
            throw new NotImplementedException();
        }

        public async Task<CheckerResult> HandleHavok(CheckerTaskMessage task, CancellationToken token)
        {
            await Task.Delay(1000);
            return CheckerResult.Ok;
        }

        private GamemasterUser CreateUser(long roundId, long teamId)
        {
            return new GamemasterUser()
            {
                RoundId = roundId,
                TeamId = teamId,
                Email = "Test",
                Password = "Test",
                Username = $"Herbert{Utils.Random.Next()}{Utils.Random.Next()}{Utils.Random.Next()}{Utils.Random.Next()}{Utils.Random.Next()}{Utils.Random.Next()}{Utils.Random.Next()}{Utils.Random.Next()}{Utils.Random.Next()}{Utils.Random.Next()}"
            };
        }
    }
}
