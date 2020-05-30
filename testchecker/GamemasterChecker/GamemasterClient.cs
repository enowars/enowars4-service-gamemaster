using Gamemaster.Models.View;
using GamemasterChecker.Models.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GamemasterChecker
{
    public class GamemasterClient : IDisposable
    {
        private readonly HttpClient HttpClient;
        private readonly string UserAgent = UserAgents.GetRandomUserAgent();
        private readonly string Scheme = "http";
        private readonly int Port = 8001;
        private IEnumerable<string>? Cookies;

        public GamemasterClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }

        public async Task<bool> Register(string address, string username, string email, string password, CancellationToken token)
        {
            var url = $"{Scheme}://{address}:{Port}/api/account/register";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("username" , username),
                    new KeyValuePair<string, string>("email" , email),
                    new KeyValuePair<string, string>("password" , password),
                })
            };
            request.Headers.Add("Accept", "application/x-www-form-urlencoded");
            request.Headers.Add("User-Agent", UserAgent);
            var response = await HttpClient.SendAsync(request, token);
            if (!response.IsSuccessStatusCode)
                return false;

            var hasCookies = response.Headers.TryGetValues("Set-Cookie", out Cookies);
            if (!hasCookies)
                return false;

            return true;
        }

        public async Task<SessionView?> CreateSession(string address, string name, string notes, string password, CancellationToken token)
        {
            var url = $"{Scheme}://{address}:{Port}/api/gamesession/create";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("name" , name),
                    new KeyValuePair<string, string>("notes" , notes),
                    new KeyValuePair<string, string>("password" , password),
                })
            };
            request.Headers.Add("Accept", "application/x-www-form-urlencoded");
            request.Headers.Add("User-Agent", UserAgent);
            request.Headers.Add("Cookie", Cookies);
            var response = await HttpClient.SendAsync(request, token);
            if (!response.IsSuccessStatusCode)
                return null;
            var responseString = await response.Content.ReadAsStringAsync(); //TODO find async variant?
            return JsonSerializer.Deserialize<SessionView>(responseString);
        }

        public async Task<bool> AddUserToSession(long sessionId, string username, CancellationToken token)
        {
            return false;
        }
    }
}
