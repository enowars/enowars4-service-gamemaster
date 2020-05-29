using EnoCore.Models;
using EnoCore.Models.Database;
using GamemasterChecker.Models.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GamemasterChecker
{
    public class GamemasterChecker : IChecker
    {
        private readonly string Scheme = "http";
        private readonly int Port = 8001;
        private IHttpClientFactory _context;
        public GamemasterChecker(IHttpClientFactory context)
        {
            _context = context;
        }
        public Task<CheckerResultMessage> HandleGetFlag(CheckerTaskMessage task)
        {
            var url = $"{Scheme}://{task.Address}:{Port}/api/account/login";
            using var client = _context.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "HttpClientFactory-Sample");
            throw new NotImplementedException();
        }
        public Task<CheckerResultMessage> HandlePutFlag(CheckerTaskMessage task)
        {
            throw new NotImplementedException();
        }
        public Task<CheckerResultMessage> HandleGetNoise(CheckerTaskMessage task)
        {
            throw new NotImplementedException();
        }
        public Task<CheckerResultMessage> HandlePutNoise(CheckerTaskMessage task)
        {
            throw new NotImplementedException();
        }
        public Task<CheckerResultMessage> HandleHavok(CheckerTaskMessage task)
        {
            throw new NotImplementedException();
        }
    }
}
