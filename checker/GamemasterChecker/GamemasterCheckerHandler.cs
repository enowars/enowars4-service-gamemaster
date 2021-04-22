namespace GamemasterChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnoCore.Checker;
    using EnoCore.Models;
    using GamemasterChecker.DbModels;
    using GamemasterChecker.Models;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class GamemasterCheckerHandler : IChecker
    {
        private readonly ILogger logger;
        private readonly GamemasterCheckerDatabase checkerDb;
        private readonly IServiceProvider serviceProvider;

        public GamemasterCheckerHandler(ILogger<GamemasterCheckerHandler> logger, GamemasterCheckerDatabase checkerDb, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.checkerDb = checkerDb;
            this.serviceProvider = serviceProvider;
            this.logger.LogInformation("GamemasterCheckerHandler()");
        }

        public async Task HandleGetFlag(CheckerTaskMessage task, CancellationToken token)
        {
            if (task.Flag == null)
            {
                throw new InvalidOperationException("Flag must not be null in getflag");
            }

            switch (task.VariantId)
            {
                case 0:
                    await this.GetFlagFromSession(task, token);
                    break;
                case 1:
                    await this.GetFlagFromToken(task, token);
                    break;
                case 2:
                    await this.GetFlagFromChat(task, token);
                    break;
            }
        }

        public Task HandleGetNoise(CheckerTaskMessage task, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public async Task HandleHavoc(CheckerTaskMessage task, CancellationToken token)
        {
           switch (task.VariantId)
            {
                case 0:
                    await this.HavocChat(task, token);
                    break;
            }
        }

        public async Task HandlePutFlag(CheckerTaskMessage task, CancellationToken token)
        {
            if (task.Flag == null)
            {
                throw new InvalidOperationException("Flag must not be null in putflag");
            }

            switch (task.VariantId)
            {
                case 0:
                    await this.PutFlagToSession(task, token);
                    break;
                case 1:
                    await this.PutFlagToToken(task, token);
                    break;
                case 2:
                    await this.PutFlagToChat(task, token);
                    break;
            }
        }

        public Task HandlePutNoise(CheckerTaskMessage task, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private async Task PutFlagToSession(CheckerTaskMessage task, CancellationToken token)
        {
            // Register a new master
            var master = Util.GenerateFakeUser(task.Flag, true);
            var users = new List<GamemasterUser>();
            using var masterClient = this.serviceProvider.GetRequiredService<GamemasterClient>();
            await masterClient.RegisterAsync(task.Address, master, token).ConfigureAwait(false);

            // Create a new session
            SessionView session = await masterClient.CreateSessionAsync(Util.GenerateFakeSessionName(), task.Flag!, Util.GenerateFakePassword(), token);
            master.SessionId = session.Id;

            // Create new users
            var newUsers = ThreadSafeRandom.Next(3) + 2;
            this.logger.LogInformation($"Target member count: {newUsers}");
            var registerTasks = new List<Task>();
            for (int i = 0; i < newUsers; i++)
            {
                var user = Util.GenerateFakeUser(task.Flag);
                users.Add(user);
                registerTasks.Add(
                    Task.Run(
                        async () =>
                        {
                            using var client = this.serviceProvider.GetRequiredService<GamemasterClient>();
                            await client.RegisterAsync(task.Address, user, token);
                        }));
            }

            foreach (var registerTask in registerTasks)
            {
                await registerTask;
            }

            this.logger.LogInformation($"Test sleep");

            await Task.Delay(3000); // wat

            // Have master add all users to session
            this.logger.LogInformation($"Adding {users.Count} users to session");
            var addSessionTasks = new List<Task>(users.Count);
            foreach (var user in users)
            {
                user.SessionId = session.Id;
                addSessionTasks.Add(masterClient.AddUserToSessionAsync(session.Id, user.Username, token));
            }

            foreach (var addSessionTask in addSessionTasks)
            {
                await addSessionTask;
            }

            // Save all users to db
            users.Add(master);
            this.logger.LogInformation($"Saving {users.Count} users to db");
            await this.checkerDb.InsertUsersAsync(users, token);
        }

        private async Task PutFlagToToken(CheckerTaskMessage task, CancellationToken token)
        {
            var master = Util.GenerateFakeUser(task.Flag);
            using var masterClient = this.serviceProvider.GetRequiredService<GamemasterClient>();
            await masterClient.RegisterAsync(task.Address, master, token).ConfigureAwait(false);

            // Create a new session
            SessionView session = await masterClient.CreateSessionAsync(Util.GenerateFakeSessionName(), Util.GenerateFakeSessionNotes(task.RelatedRoundId.Value), "password", token);
            byte[] imgdata = new byte[64];
            ThreadSafeRandom.NextBytes(imgdata);
            var uuid = await masterClient.AddTokenAsync(Util.GenerateFakeTokenName(), task.Flag!, true, imgdata, token);
            await this.checkerDb.AddTokenUUIDAsync(task.Flag!, uuid, token);
            await this.checkerDb.AddUserAsync(master, token);
        }

        private async Task PutFlagToChat(CheckerTaskMessage task, CancellationToken token)
        {
            var user1 = Util.GenerateFakeUser(null);
            var user2 = Util.GenerateFakeUser(task.Flag);
            var user3 = Util.GenerateFakeUser(null);
            using var client1 = this.serviceProvider.GetRequiredService<GamemasterClient>();
            using var client2 = this.serviceProvider.GetRequiredService<GamemasterClient>();
            using var client3 = this.serviceProvider.GetRequiredService<GamemasterClient>();
            var tl1 = client1.RegisterAsync(task.Address, user1, token);
            var tl2 = client2.RegisterAsync(task.Address, user2, token);
            var tl3 = client3.RegisterAsync(task.Address, user3, token);
            await tl1;
            await tl2;
            await tl3;
            var s = await client3.CreateSessionAsync(Util.GenerateFakeSessionName(), "n", "password", token);
            var ta1 = client3.AddUserToSessionAsync(s!.Id, user2.Username, token);
            var ta2 = client3.AddUserToSessionAsync(s!.Id, user1.Username, token);
            await ta1;
            await ta2;
            await using GamemasterSignalRClient signalrclient1 = client1.CreateSignalRConnection(task.Address, null, null, this.serviceProvider, token);
            await using GamemasterSignalRClient signalrclient2 = client1.CreateSignalRConnection(task.Address, null, null, this.serviceProvider, token);
            var tc1 = signalrclient1.Connect();
            var tc2 = signalrclient2.Connect();
            await tc1;
            await tc2;
            var tj1 = signalrclient1.Join(s.Id, token);
            var tj2 = signalrclient2.Join(s.Id, token);
            await tj1;
            await tj2;
            await signalrclient1.SendMessage(task.Flag!, token); // TODO we should check of the other client actually receives this
            user2.SessionId = s.Id;
            await this.checkerDb.AddUserAsync(user2, token);
        }

        private async Task GetFlagFromSession(CheckerTaskMessage task, CancellationToken token)
        {
            var shorterToken = new CancellationTokenSource((int)(task.Timeout * 0.9)).Token;
            this.logger.LogInformation($"Fetching Users");
            var users = await this.checkerDb.GetUsersAsync(task.Flag!, token);
            this.logger.LogInformation($"found {users.Count}");
            if (users.Count <= 0)
            {
                throw new MumbleException("Putflag failed");
            }

            using var client = this.serviceProvider.GetRequiredService<GamemasterClient>();
            var master = users.Where(u => u.IsMaster).SingleOrDefault();
            if (master == null)
            {
                throw new MumbleException("Putflag failed");
            }

            if (task.CurrentRoundId == task.RelatedRoundId)
            {
                await this.CheckSessionList(task.Address, client, master, token);
            }

            await client.LoginAsync(task.Address, users[0], token);
            ExtendedSessionView session = await client.FetchSessionAsync(users[0].SessionId, token);
            this.logger.LogInformation($"Retrieved Flag is {session.Notes}, Requested Flag is {task.Flag}");
            if (!session.Notes.Equals(task.Flag))
            {
                throw new MumbleException("Flag not found in session note");
            }
        }

        private async Task GetFlagFromToken(CheckerTaskMessage task, CancellationToken token)
        {
            this.logger.LogInformation("Fetching Token From Db");
            string? gtoken = await this.checkerDb.GetTokenUUIDAsync(task.Flag!, token);
            if (gtoken == null)
            {
                throw new MumbleException("Putflag failed");
            }

            var users = await this.checkerDb.GetUsersAsync(task.Flag!, token);
            if (users == null || users.Count != 1)
            {
                this.logger.LogInformation($"Master User for the Token not found in Db, or multiple found for the flag: Count:{((users != null) ? users.Count : -1)}");
                throw new MumbleException("Putflag failed");
            }

            using var mclient = this.serviceProvider.GetRequiredService<GamemasterClient>();
            TokenStrippedView retrievedToken = await mclient.CheckTokenAsync(task.Address, gtoken, token);

            if (!retrievedToken.Description.Equals(task.Flag))
            {
                throw new MumbleException("Flag not found in token");
            }
        }

        private async Task GetFlagFromChat(CheckerTaskMessage task, CancellationToken token)
        {
            var shorterToken = new CancellationTokenSource((int)(task.Timeout * 0.9)).Token;
            var users = await this.checkerDb.GetUsersAsync(task.Flag!, token);
            if (users.Count <= 0)
            {
                throw new MumbleException("Putflag failed");
            }

            var user1 = users[0];
            this.logger.LogInformation($"GetFlagFromChat -  Name:\"{user1.Username}\", Password:\"{user1.Password}\", SessionId:\"{user1.SessionId}\"");
            using var client1 = this.serviceProvider.GetRequiredService<GamemasterClient>();
            await client1.LoginAsync(task.Address, user1, token);
            var tcs = new TaskCompletionSource<bool>();
            await using GamemasterSignalRClient signalrclient1 = client1.CreateSignalRConnection(task.Address, tcs, task.Flag, this.serviceProvider, shorterToken);
            await signalrclient1.Connect();
            var tj1 = signalrclient1.Join(user1.SessionId, token);
            await tj1;
            await tcs.Task;
        }

        private async Task HavocChat(CheckerTaskMessage task, CancellationToken token)
        {
            var shorterToken = new CancellationTokenSource((int)(task.Timeout * 0.9)).Token;
            var user1 = Util.GenerateFakeUser(null);
            var user2 = Util.GenerateFakeUser(null);
            var user3 = Util.GenerateFakeUser(null);
            using var client1 = this.serviceProvider.GetRequiredService<GamemasterClient>();
            using var client2 = this.serviceProvider.GetRequiredService<GamemasterClient>();
            using var client3 = this.serviceProvider.GetRequiredService<GamemasterClient>();
            var tl1 = client1.RegisterAsync(task.Address, user1, token);
            var tl2 = client2.RegisterAsync(task.Address, user2, token);
            var tl3 = client3.RegisterAsync(task.Address, user3, token);
            await tl1;
            await tl2;
            await tl3;
            var s = await client3.CreateSessionAsync(Util.GenerateFakeSessionName(), "n", "password", token);
            var ta1 = client3.AddUserToSessionAsync(s.Id, user2.Username, token);
            var ta2 = client3.AddUserToSessionAsync(s.Id, user1.Username, token);
            await ta1;
            await ta2;
            var tcs = new TaskCompletionSource<bool>();
            var message = Util.GenerateFakeChatMessage();
            await using var src1 = client1.CreateSignalRConnection(task.Address, null, null, this.serviceProvider, shorterToken);
            await using var src2 = client2.CreateSignalRConnection(task.Address, tcs, message, this.serviceProvider, shorterToken);
            var tc1 = src1.Connect();
            var tc2 = src2.Connect();
            await tc1;
            await tc2;
            var tj1 = src1.Join(s.Id, token);
            var tj2 = src2.Join(s.Id, token);
            await tj1;
            await tj2;
            await src1.SendMessage(message, token);
            await tcs.Task;
        }

        private async Task CheckSessionList(string address, GamemasterClient client, GamemasterUser master, CancellationToken token)
        {
            this.logger.LogDebug($"CheckSessionList looking for session of {master.Username} ({master.SessionId})");
            var i = 0;
            while (!token.IsCancellationRequested && i < 100 * 10)
            {
                var sessions = await client.FetchSessionList(address, i, 100, token);
                var first = sessions.FirstOrDefault();
                var last = sessions.LastOrDefault();
                if (first == null || last == null)
                {
                    throw new MumbleException("Session list empty");
                }

                if (master.SessionId < last.Id)
                {
                    this.logger.LogDebug($"CheckSessionList fetching more, {master.SessionId} < {last.Id}");
                    i += 100;
                    continue;
                }

                foreach (var session in sessions)
                {
                    if (session.Id == master.SessionId && master.Username == session.OwnerName)
                    {
                        return;
                    }
                }
            }

            throw new MumbleException("Could not find Session");
        }
    }
}
