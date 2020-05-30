using EnoCore.Models;
using EnoCore.Models.Database;
using Gamemaster.Models.View;
using GamemasterChecker.Models.Json;
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
            var users = await Db.GetUsersAsync(task.Round - 1, task.TeamId, token);
            users = users.Where(u => Utils.Random.Next() % 2 == 0).ToList();

            // Register a new master
            var master = CreateUser();
            using var masterClient = new GamemasterClient(HttpFactory.CreateClient("default"), task.Address);
            bool result;
            try
            {
                result = await masterClient.RegisterAsync(master, token);
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
            var newUsers = Utils.Random.Next(4, 8) - users.Count;
            var registerTasks = new List<Task<bool>>();
            for (int i = 0; i < newUsers; i++)
            {
                var user = CreateUser();
                users.Add(user);
                registerTasks.Add(Task.Run(async () =>
                {
                    using var client = new GamemasterClient(HttpFactory.CreateClient("default"), task.Address);
                    return await client.RegisterAsync(user, token);
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
            foreach (var user in users)
            {
                await masterClient.AddUserToSession(session.Id, user.Username, token);
            }

            // Save all users to db
            foreach (var user in users)
            {
                await Db.AddUserAsync(user, token);
            }
            return CheckerResult.Ok;
        }

        public async Task<CheckerResult> HandleGetFlag(CheckerTaskMessage task, CancellationToken Token)
        {
            await Task.Delay(1000);
            return CheckerResult.Ok;
        }

        public async Task<CheckerResult> HandleGetNoise(CheckerTaskMessage task, CancellationToken Token)
        {
            await Task.Delay(1000);
            throw new NotImplementedException();
        }

        public async Task<CheckerResult> HandlePutNoise(CheckerTaskMessage task, CancellationToken Token)
        {
            await Task.Delay(1000);
            throw new NotImplementedException();
        }

        public async Task<CheckerResult> HandleHavok(CheckerTaskMessage task, CancellationToken Token)
        {
            await Task.Delay(1000);
            return CheckerResult.Ok;
        }

        private GamemasterUser CreateUser()
        {
            return new GamemasterUser()
            {
                Email = "Test",
                Password = "Test",
                Username = $"Herbert{Utils.Random.Next()}"
            };
        }
    }
}
