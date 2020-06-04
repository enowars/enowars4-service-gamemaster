using Gamemaster.Models.View;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger Logger;
        private readonly string Address;
        private readonly HttpClient HttpClient;
        private readonly string UserAgent = UserAgents.GetRandomUserAgent();
        private readonly string Scheme = "http";
        private readonly int Port = 8001;
        private readonly GamemasterUser User;
        private IEnumerable<string>? Cookies;
        private readonly JsonSerializerOptions JsonOptions;

        public GamemasterClient(HttpClient httpClient, string address, GamemasterUser user, ILogger logger)
        {
            Logger = logger;
            User = user;
            Address = address;
            HttpClient = httpClient;
            JsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }

        public async Task<bool> RegisterAsync(CancellationToken token)
        {
            var url = $"{Scheme}://{Address}:{Port}/api/account/register";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("username" , User.Username),
                    new KeyValuePair<string, string>("email" , User.Email),
                    new KeyValuePair<string, string>("password" , User.Password)
                })
            };
            request.Headers.Add("Accept", "application/x-www-form-urlencoded");
            request.Headers.Add("User-Agent", UserAgent);
            var response = await HttpClient.SendAsync(request, token);
            Logger.LogInformation($"{url} returned {response.StatusCode}");
            if (!response.IsSuccessStatusCode)
                return false;

            if (this.Cookies != null)
                throw new InvalidOperationException("User already has cookies");
            var hasCookies = response.Headers.TryGetValues("Set-Cookie", out Cookies);
            if (!hasCookies)
                return false;

            return true;
        }
        public async Task<bool> LoginAsync(CancellationToken token)
        {
            var url = $"{Scheme}://{Address}:{Port}/api/account/login";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("username" , User.Username),
                    new KeyValuePair<string, string>("password" , User.Password)
                })
            };
            request.Headers.Add("Accept", "application/x-www-form-urlencoded");
            request.Headers.Add("User-Agent", UserAgent);
            var response = await HttpClient.SendAsync(request, token);
            Logger.LogInformation($"{url} returned {response.StatusCode}");
            if (!response.IsSuccessStatusCode)
                return false;

            if (this.Cookies != null)
                throw new InvalidOperationException("User already has cookies");
            var hasCookies = response.Headers.TryGetValues("Set-Cookie", out Cookies);
            if (!hasCookies)
                return false;

            return true;
        }

        public async Task<SessionView?> CreateSessionAsync(string name, string notes, string password, CancellationToken token)
        {
            var url = $"{Scheme}://{Address}:{Port}/api/gamesession/create";
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
            Logger.LogInformation($"{url} returned {response.StatusCode}");
            if (!response.IsSuccessStatusCode)
                return null;
            var responseString = await response.Content.ReadAsStringAsync(); //TODO find async variant?
            return JsonSerializer.Deserialize<SessionView>(responseString, JsonOptions);
        }

        public async Task<bool> AddUserToSession(long sessionId, string username, CancellationToken token)
        {
            var url = $"{Scheme}://{Address}:{Port}/api/gamesession/adduser";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("sessionid" , sessionId.ToString()),
                    new KeyValuePair<string, string>("username" , username)
                })
            };
            request.Headers.Add("Accept", "application/x-www-form-urlencoded");
            request.Headers.Add("User-Agent", UserAgent);
            request.Headers.Add("Cookie", Cookies);
            var response = await HttpClient.SendAsync(request, token);
            Logger.LogInformation($"{url} returned {response.StatusCode}");
            if (!response.IsSuccessStatusCode)
                return false;
            return true;
        }
    }
}
