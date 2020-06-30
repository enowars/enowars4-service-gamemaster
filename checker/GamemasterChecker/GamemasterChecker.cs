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

        public async Task HandlePutFlag(CheckerTaskMessage task, CancellationToken token)
        {
            if (task.Flag == null)
                throw new InvalidOperationException("Flag must not be null in putflag");
            var op = task.FlagIndex % 3;
            if (op == 0)
                await PutFlagToSession(task, token);
            else if (op == 1)
                await PutFlagToToken(task, token);
            else if (op == 2)
                await PutFlagToChat(task, token);
        }
        public async Task HandleGetFlag(CheckerTaskMessage task, CancellationToken token)
        {
            if (task.Flag == null)
                throw new InvalidOperationException("Flag must not be null in getflag");
            var op = task.FlagIndex % 3;
            if (op == 0)
                await GetFlagFromSession(task, token);
            else if (op == 1)
                await GetFlagFromToken(task, token);
            else if (op == 2)
                await GetFlagFromChat(task, token);
        }

        public Task HandleGetNoise(CheckerTaskMessage task, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task HandlePutNoise(CheckerTaskMessage task, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public async Task HandleHavoc(CheckerTaskMessage task, CancellationToken token)
        {
            await HavocChat(task, token);
        }
        private string GetSessionName()
        {
            var Session = FakeUsers.GetFakeSession();
            Logger.LogInformation($"GetSessionName returned {Session}");
            return Session;
        }
        private string GetChatMessage()
        {
            var msg = FakeUsers.GetFakeChat();
            Logger.LogInformation($"GetChatMessage returned {msg}");
            return msg;
        }

        private async Task PutFlagToChat(CheckerTaskMessage task, CancellationToken token)
        {
            var user1 = FakeUsers.GetFakeUser(-1, -1, null);
            var user2 = FakeUsers.GetFakeUser(-1, -1, task.Flag);
            var user3 = FakeUsers.GetFakeUser(-1, -1, null);
            var client1 = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, user1, Logger);
            var client2 = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, user2, Logger);
            var client3 = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, user3, Logger);
            var tl1 = client1.RegisterAsync(token);
            var tl2 = client2.RegisterAsync(token);
            var tl3 = client3.RegisterAsync(token);
            await tl1; await tl2; await tl3;
            var s = await client3.CreateSessionAsync(GetSessionName(), "n", "password", token);
            var ta1 = client3.AddUserToSessionAsync(s!.Id, user2.Username, token);
            var ta2 = client3.AddUserToSessionAsync(s!.Id, user1.Username, token);
            await ta1; await ta2;
            await using GamemasterSignalR src1 = new GamemasterSignalR(task.Address, user1, Logger, null, 0, null, client1, token);
            await using GamemasterSignalR src2 = new GamemasterSignalR(task.Address, user2, Logger, null, 0, null, client2, token);
            var tc1 = src1.Connect();
            var tc2 = src2.Connect();
            await tc1; await tc2;
            var tj1 = src1.Join(s.Id, token);
            var tj2 = src2.Join(s.Id, token);
            await tj1; await tj2;
            await src1.SendMessage(task.Flag!, token);
            user2.SessionId = s.Id;
            await Db.AddUserAsync(user2, token);
        }
        private async Task GetFlagFromChat(CheckerTaskMessage task, CancellationToken token)
        {
            var users = await Db.GetUsersAsync(task.Flag!, token);
            if (users.Count <= 0)
                throw new MumbleException("Putflag failed");
            var user1 = users[0];
            Logger.LogInformation($"GetFlagFromChat -  Name:\"{user1.Username}\", Password:\"{user1.Password}\", SessionId:\"{user1.SessionId}\"");
            var client1 = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, user1, Logger);
            await client1.LoginAsync(token);
            var tcs = new TaskCompletionSource<bool>();
            await using GamemasterSignalR src1 = new GamemasterSignalR(task.Address, user1, Logger, tcs, 0, task.Flag, client1, token);
            await src1.Connect();
            var tj1 = src1.Join(user1.SessionId, token);
            await tj1;
            await tcs.Task;
        }
        private async Task PutFlagToSession(CheckerTaskMessage task, CancellationToken token)
        {
            // Get random user subset from last round
            var users = new List<GamemasterUser>();
            await Db.GetUsersAsync(task.RoundId - 1, task.TeamId, token);
            Logger.LogDebug($"Users from Last Round: {users.Count()}");
            users = users.Where(u => Utils.Random.Next() % 2 == 0).ToList();
            Logger.LogDebug($"Users after pruning: {users.Count()}");
            // Register a new master
            var master = FakeUsers.GetFakeUser(task.RoundId, task.TeamId, task.Flag);
            using var masterClient = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, master, Logger);
            await masterClient.RegisterAsync(token).ConfigureAwait(false);
            // Create a new session
            SessionView session = await masterClient.CreateSessionAsync(GetSessionName(), task.Flag!, "password", token);

            // Create new users
            var usersToCreate = Utils.Random.Next(4, 8);
            var newUsers = usersToCreate - users.Count;
            Logger.LogInformation($"Target User Count: {usersToCreate}");
            var registerTasks = new List<Task>();
            for (int i = 0; i < newUsers; i++)
            {
                var user = FakeUsers.GetFakeUser(task.RoundId, task.TeamId, task.Flag);
                users.Add(user);
                registerTasks.Add(Task.Run(async () =>
                {
                    using var client = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, user, Logger);
                    await client.RegisterAsync(token);
                }));
            }
            foreach (var registerTask in registerTasks)
            {
                await registerTask;
            }
            // Have master add all users to session

            Logger.LogInformation($"Adding {users.Count} users to session");
            var addSessionTasks = new List<Task>();
            foreach (var user in users)
            {
                addSessionTasks.Add(masterClient.AddUserToSessionAsync(session.Id, user.Username, token));
            }
            foreach (var addSessionTask in addSessionTasks)
            {
                await addSessionTask;
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
        }
        private async Task PutFlagToToken(CheckerTaskMessage task, CancellationToken token)
        {
            var smaster = FakeUsers.GetFakeUser(task.RoundId, task.TeamId, task.Flag);
            //smaster.Username = "Herbert" + task.Flag + Environment.TickCount.ToString();
            using var masterClient = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, smaster, Logger);
            await masterClient.RegisterAsync(token).ConfigureAwait(false);

            // Create a new session
            SessionView session = await masterClient.CreateSessionAsync("name", "No 🏳️‍🌈 here, go away...", "password", token);
            byte[] imgdata = new byte[64];
            Utils.Random.NextBytes(imgdata);
            var uuid = await masterClient.AddTokenAsync("name", task.Flag!, true, imgdata, token);
            await Db.AddTokenUUIDAsync(task.Flag!, uuid, token);
            await Db.AddUserAsync(smaster, token);
        }
        private async Task GetFlagFromSession(CheckerTaskMessage task, CancellationToken token)
        {
            Logger.LogInformation($"Fetching Users with relrID{task.RelatedRoundId}, tIdis:{task.TeamId}");
            var users = await Db.GetUsersAsync(task.Flag!, token);
            Logger.LogInformation($"found {users.Count}");
            if (users.Count <= 0)
                throw new MumbleException("Putflag failed");
            using var client = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, users[0], Logger);
            await client.LoginAsync(token);
            ExtendedSessionView session = await client.FetchSessionAsync(users[0].SessionId, token);
            Logger.LogInformation($"Retrieved Flag is {session.Notes}, Requested Flag is {task.Flag}");
            if (!session.Notes.Equals(task.Flag))
                throw new MumbleException("Flag not found in session note");
        }
        private async Task GetFlagFromToken(CheckerTaskMessage task, CancellationToken token)
        {
            Logger.LogInformation("Fetching Token From Db");
            string gtoken = await Db.GetTokenUUIDAsync(task.Flag!, token);
            if (gtoken == "")
                throw new MumbleException("Putflag failed");
            var smaster = await Db.GetUsersAsync(task.Flag!, token);
            if (smaster == null || smaster.Count != 1)
            {
                Logger.LogInformation($"Master User for the Token not found in Db, or multiple found for the flag: Count:{((smaster!=null) ? smaster.Count:-1)}");
                throw new MumbleException("Putflag failed");
            }
            var mclient = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, smaster[0], Logger);
            TokenStrippedView retrievedToken = await mclient.CheckTokenAsync(gtoken, token);

            if (!retrievedToken.Description.Equals(task.Flag))
                throw new MumbleException("Flag not found in token");
        }
        private async Task HavocChat(CheckerTaskMessage task, CancellationToken token)
        {
            var user1 = FakeUsers.GetFakeUser(-1, -1, null);
            var user2 = FakeUsers.GetFakeUser(-1, -1, null);
            var user3 = FakeUsers.GetFakeUser(-1, -1, null);
            var client1 = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, user1, Logger);
            var client2 = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, user2, Logger);
            var client3 = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, user3, Logger);
            var tl1 = client1.RegisterAsync(token);
            var tl2 = client2.RegisterAsync(token);
            var tl3 = client3.RegisterAsync(token);
            await tl1; await tl2; await tl3;
            var s = await client3.CreateSessionAsync(GetSessionName(), "n", "password", token);
            var ta1 = client3.AddUserToSessionAsync(s.Id, user2.Username, token);
            var ta2 = client3.AddUserToSessionAsync(s.Id, user1.Username, token);
            await ta1; await ta2;
            var tcs = new TaskCompletionSource<bool>();
            var message = GetChatMessage();
            await using GamemasterSignalR src1 = new GamemasterSignalR(task.Address, user1, Logger, null, 0, null, client1, token);
            await using GamemasterSignalR src2 = new GamemasterSignalR(task.Address, user2, Logger, tcs, 1, message, client2, token);
            var tc1 = src1.Connect();
            var tc2 = src2.Connect();
            await tc1; await tc2;
            var tj1 = src1.Join(s.Id, token);
            var tj2 = src2.Join(s.Id, token);
            await tj1; await tj2;
            await src1.SendMessage(message, token);
            await tcs.Task;
        }
    }
}
