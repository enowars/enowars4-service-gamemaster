using EnoCore.Models;
using EnoCore.Models.Database;
using GamemasterChecker.Models.Json;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GamemasterChecker
{
    public class GamemasterChecker : IChecker
    {
        private readonly IHttpClientFactory HttpFactory;

        public GamemasterChecker(IHttpClientFactory httpFactory)
        {
            HttpFactory = httpFactory;
        }

        public async Task<CheckerResult> HandleGetFlag(CheckerTaskMessage task, CancellationToken Token)
        {
            return CheckerResult.Ok;
        }
        public async Task<CheckerResult> HandlePutFlag(CheckerTaskMessage task, CancellationToken token)
        {
            using var client = new GamemasterClient(HttpFactory.CreateClient("default"));
            var result = await client.Register(task.Address, "test1", "test", "test", token);
            if (!result)
                return CheckerResult.Mumble;
            await client.CreateSession(task.Address, "name", "notes", "password", token);
            return CheckerResult.Ok;
        }
        public async Task<CheckerResult> HandleGetNoise(CheckerTaskMessage task, CancellationToken Token)
        {
            throw new NotImplementedException();
        }
        public async Task<CheckerResult> HandlePutNoise(CheckerTaskMessage task, CancellationToken Token)
        {
            throw new NotImplementedException();
        }
        public async Task<CheckerResult> HandleHavok(CheckerTaskMessage task, CancellationToken Token)
        {
            throw new NotImplementedException();
        }
    }
}
