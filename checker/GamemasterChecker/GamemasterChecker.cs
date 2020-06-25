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

        public async Task<CheckerResultMessage> HandlePutFlag(CheckerTaskMessage task, CancellationToken token)
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
        public async Task<CheckerResultMessage> HandleGetFlag(CheckerTaskMessage task, CancellationToken token)
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

        public async Task<CheckerResultMessage> HandleGetNoise(CheckerTaskMessage task, CancellationToken token)
        {
            return new CheckerResultMessage()
            {
                Result = CheckerResult.OK,
                Message = "Finished Successful"
            };
        }

        public async Task<CheckerResultMessage> HandlePutNoise(CheckerTaskMessage task, CancellationToken token)
        {
            return new CheckerResultMessage()
            {
                Result = CheckerResult.OK,
                Message = "Finished Successful"
            };
        }

        public async Task<CheckerResultMessage> HandleHavok(CheckerTaskMessage task, CancellationToken token)
        {
            return await HavokChat(task, token);
            /*
            return new CheckerResultMessage()
            {
                Result = CheckerResult.OK,
                Message = "Finished Successful"
            };*/
        }
        private string GetSessionName()
        {
            return "blaaaah";
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
        private async Task<CheckerResultMessage> PutFlagToSession(CheckerTaskMessage task, CancellationToken token)
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
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.OFFLINE,
                    Message = $"Register OFFLINE"
                };
            }
            if (!result)
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.MUMBLE,
                    Message = $"Register Failed - returned invalid Statuscode or no cookies"
                };
            // Create a new session
            SessionView? session;
            try
            {
                session = await masterClient.CreateSessionAsync("name", task.Flag!, "password", token);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToFancyString());
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.OFFLINE,
                    Message = $"Create Session OFFLINE"
                };
            }
            if (session == null || session.Id == 0)
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.MUMBLE,
                    Message = $"CreateSession did not finish correctly."
                };

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
                        return new CheckerResultMessage()
                        {
                            Result = CheckerResult.MUMBLE,
                            Message = $"Register Failed - returned invalid Statuscode or no cookies"
                        };
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToFancyString());
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.OFFLINE,
                    Message = $"Register OFFLINE"
                };
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
                        return new CheckerResultMessage()
                        {
                            Result = CheckerResult.MUMBLE,
                            Message = $"Adding users to session Failed"
                        };
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToFancyString());
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.OFFLINE,
                    Message = $"Adding users to session OFFLINE"
                };
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
            return new CheckerResultMessage()
            {
                Result = CheckerResult.OK,
                Message = $"Checker returned ok"
            };
        }
        private async Task<CheckerResultMessage> PutFlagToToken(CheckerTaskMessage task, CancellationToken token)
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
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.OFFLINE,
                    Message = $"Register OFFLINE"
                };
            }
            if (!result)
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.MUMBLE,
                    Message = $"Register Failed - returned invalid Statuscode or no cookies"
                };

            // Create a new session
            SessionView? session;
            try
            {
                session = await masterClient.CreateSessionAsync("name", "No 🏳️‍🌈 here, go away...", "password", token);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToFancyString());
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.OFFLINE,
                    Message = $"Create Session OFFLINE"
                };
            }
            if (session == null || session.Id == 0)
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.OFFLINE,
                    Message = $"CreateSession did not finish correctly."
                };
            byte[] imgdata = new byte[64];
            Utils.Random.NextBytes(imgdata);
            var UUID = await masterClient.AddTokenAsync("name", task.Flag!, true, imgdata, token);
            if (UUID==null  || !isValid(UUID))
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.MUMBLE,
                    Message = $"Addtoken return invalid."
                };
            await Db.AddTokenUUIDAsync(task.Flag!, UUID, token);
            await Db.AddUserAsync(smaster, token);
            return new CheckerResultMessage()
            {
                Result = CheckerResult.OK,
                Message = $"Checker returned ok"
            };
        }
        private async Task<CheckerResultMessage> GetFlagFromSession(CheckerTaskMessage task, CancellationToken token)
        {
            Logger.LogInformation($"Fetching Users with relrID{task.RelatedRoundId}, tIdis:{task.TeamId}");
            var users = await Db.GetUsersAsync(task.Flag, token);
            Logger.LogInformation($"found {users.Count}");                       //################################## MUMBLE?
            if (users.Count <= 0)
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.MUMBLE,
                    Message = $"Could not retrieve data"
                };
            using var client = new GamemasterClient(HttpFactory.CreateClient(task.TeamId.ToString()), task.Address, users[0], Logger);
            try
            {
                if (!(await client.LoginAsync(token))) 
                    return new CheckerResultMessage()
                    {
                        Result = CheckerResult.MUMBLE,
                        Message = $"Login failed"
                    };
            }
            catch (Exception)
            {
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.OFFLINE,
                    Message = $"Login OFFLINE"
                };
            }
            ExtendedSessionView session;
            try
            {
                session = await client.FetchSessionAsync(users[0].SessionId, token);
            }
            catch (Exception)
            {
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.OFFLINE,
                    Message = $"GetSession OFFLINE"
                };
            }
            Logger.LogInformation($"Retrieved Flag is {session.Notes}");
            Logger.LogInformation($"Requested Flag is {task.Flag}");
            if (session.Notes.Equals(task.Flag))
            {
                Logger.LogInformation("Flags are Equal");
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.OK,
                    Message = $"Checker returned ok"
                };
            }
            Logger.LogInformation("Flags are not Equal");
            return new CheckerResultMessage()
            {
                Result = CheckerResult.MUMBLE,
                Message = $"Session data inconsistent"
            };
        }
        private async Task<CheckerResultMessage> GetFlagFromToken(CheckerTaskMessage task, CancellationToken token)
        {
            Logger.LogInformation("Fetching Token From Db");
            string gtoken = await Db.GetTokenUUIDAsync(task.Flag!, token);
            if (gtoken == "")
            {
                Logger.LogInformation("No Token Found in Db");
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.MUMBLE,
                    Message = $"Putflag likely failed"
                };
            }
            var smaster = await Db.GetUsersAsync(task.Flag!, token);
            if (smaster == null || smaster.Count != 1)
            {
                Logger.LogInformation($"Master User for the Token not found in Db, or multiple found for the flag: Count:{((smaster!=null) ? smaster.Count:-1)}");
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.MUMBLE,
                    Message = $"Putflag likely failed"
                };
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
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.OFFLINE,
                    Message = $"Token Info failed"
                };
            }
            
            if (retrievedToken.Description.Equals(task.Flag))
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.OK,
                    Message = $"Checker returned ok"
                };
            else
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.MUMBLE,
                    Message = $"Token data inconsistent"
                };
        }
        private bool isValid (string UUID)
        {
            if (UUID.Length != 128) return false;
            return true;
        }
        private async Task<CheckerResultMessage> HavokChat(CheckerTaskMessage task, CancellationToken token)
        {
            var user1 = CreateUser(-1, -1, "");
            var user2 = CreateUser(-1, -1, "");
            var user3 = CreateUser(-1, -1, "");
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
            GamemasterSignalR src1 = new GamemasterSignalR(task.Address, user1, Logger, null, token);
            GamemasterSignalR src2 = new GamemasterSignalR(task.Address, user2, Logger, tcs, token);
            var tc1 = src1.Connect();
            var tc2 = src2.Connect();
            await tc1; await tc2;
            var tj1 = src1.Join(s.Id, token);
            var tj2 = src2.Join(s.Id, token);
            await tj1; await tj2;
            await src1.SendMessage("blabla", token);
            if (await tcs.Task)
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.OK,
                    Message = $"Checker returned ok"
                };
            else
                return new CheckerResultMessage()
                {
                    Result = CheckerResult.MUMBLE,
                    Message = $"Chat Broken"
                };
        }
    }
}
