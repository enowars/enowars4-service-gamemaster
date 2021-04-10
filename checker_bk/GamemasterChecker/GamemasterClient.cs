using Bogus.DataSets;
using EnoCore.Utils;
using Gamemaster.Models.View;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
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
        private readonly string UserAgent = FakeUsers.GetUserAgent();
        private readonly string Scheme = "http";
        private readonly int Port = 8001;
        private readonly GamemasterUser User;
        public IEnumerable<string>? Cookies;
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

        public async Task RegisterAsync(CancellationToken token)
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
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.SendAsync(request, token);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"{User.Username} failed: {e.ToFancyString()}");
                throw new OfflineException("Registration failed");
            }
            Logger.LogInformation($"{url} returned {response.StatusCode}");
            if (!response.IsSuccessStatusCode)
                throw new MumbleException("Registration failed");

            if (Cookies != null)
                throw new InvalidOperationException("User already has cookies");
            var hasCookies = response.Headers.TryGetValues("Set-Cookie", out Cookies);
            if (!hasCookies)
                throw new MumbleException("Registration failed");
        }
        public async Task LoginAsync(CancellationToken token)
        {
            Logger.LogInformation($"Login {User.Username}:{User.Password}");
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
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.SendAsync(request, token);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"{User.Username} failed: {e.ToFancyString()}");
                throw new OfflineException("Login failed");
            }
            if (!response.IsSuccessStatusCode)
                throw new MumbleException("Login failed");
            if (Cookies != null)
                throw new InvalidOperationException("User already has cookies");
            var hasCookies = response.Headers.TryGetValues("Set-Cookie", out Cookies);
            if (!hasCookies)
                throw new MumbleException("Login failed");
        }

        public async Task<SessionView> CreateSessionAsync(string name, string notes, string password, CancellationToken token)
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
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.SendAsync(request, token);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"{User.Username} failed: {e.ToFancyString()}");
                throw new OfflineException("Create session failed");
            }
            Logger.LogInformation($"{url} returned {response.StatusCode}");
            if (!response.IsSuccessStatusCode)
                throw new MumbleException("Create session failed");
            var responseString = await response.Content.ReadAsStringAsync(); //TODO find cancellable variant or verify that it is already cancelled
            var view = JsonSerializer.Deserialize<SessionView>(responseString, JsonOptions);
            if (view.Id <= 0 || view.Name == null || view.OwnerName == null || view.Timestamp == null)
            {
                Logger.LogWarning($"{User.Username} create session failed: invalid view");
                throw new MumbleException("Create session failed");
            }
            return view;
        }
        public async Task<string> AddTokenAsync(string name, string description, bool isPrivate, byte[] ImageData, CancellationToken token)
        {
            var url = $"{Scheme}://{Address}:{Port}/api/account/addtoken";
            var ImageContent = new ByteArrayContent(ImageData);
            ImageContent.Headers.Add("Content-Type", "image/png");
            ImageContent.Headers.Add("Content-Disposition", "form-data; name=\"icon\"; filename=\"Arrow.png\"");
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new MultipartFormDataContent
                {
                    { new StringContent(name), "\"name\"" },
                    { new StringContent(description), "\"description\"" },
                    { new StringContent(isPrivate.ToString().ToLower()), "\"isPrivate\"" },
                    { ImageContent, "\"icon\"", "\"Arrow.png\"" }
                }
            };
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("User-Agent", UserAgent);
            request.Headers.Add("Cookie", Cookies);
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.SendAsync(request, token);
                Logger.LogDebug($"{url} returned {response.StatusCode}");
            }
            catch (Exception e)
            {
                Logger.LogWarning($"{User.Username} failed: {e.ToFancyString()}");
                throw new OfflineException("Create token failed");
            }
            if (!response.IsSuccessStatusCode)
                throw new MumbleException("Create token failed");
            var responseString = await response.Content.ReadAsStringAsync(); //TODO find cancellable variant or verify that it is already cancelled
            var uuid = responseString.Replace("\"", "");
            if (!IsValidUuid(uuid))
                throw new MumbleException("Create token failed");
            return uuid;
        }
        public async Task<TokenStrippedView> CheckTokenAsync(string UUID, CancellationToken token)
        {
            var url = $"{Scheme}://{Address}:{Port}/api/token/info";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("UUID" , UUID),
                })
            };
            request.Headers.Add("Accept", "application/x-www-form-urlencoded");
            request.Headers.Add("User-Agent", UserAgent);
            if (Cookies != null)
                request.Headers.Add("Cookie", Cookies);
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.SendAsync(request, token);
                Logger.LogInformation($"{url} returned {response.StatusCode}");
            }
            catch (Exception e)
            {
                Logger.LogWarning($"{User.Username} failed: {e.ToFancyString()}");
                throw new OfflineException("Get token info failed");
            }
            TokenStrippedView tsv;
            try
            {
                var responseString = await response.Content.ReadAsStringAsync(); //TODO find async variant?
                tsv = JsonSerializer.Deserialize<TokenStrippedView>(responseString, JsonOptions);

            }
            catch
            {
                throw new MumbleException("Failed to Parse Result of /api/token/info");
            }
            if (tsv.Name == null || tsv.OwnerName == null || tsv.UUID == null || tsv.Description == null)
                throw new MumbleException("Get token info failed");
            return tsv;
        }
        public async Task<SessionView[]> FetchSessionList(long skip, long take, CancellationToken token)
        {
            var url = $"{Scheme}://{Address}:{Port}/api/gamesession/listrecent";
            UriBuilder builder = new UriBuilder(url);
            builder.Query = $"skip={skip}&take={take}";
            var request = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
            request.Headers.Add("Accept", "application/x-www-form-urlencoded");
            request.Headers.Add("User-Agent", UserAgent);
            if (Cookies != null)
                request.Headers.Add("Cookie", Cookies);
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.SendAsync(request, token);
                Logger.LogInformation($"{url} returned {response.StatusCode}");
            }
            catch (Exception e)
            {
                Logger.LogWarning($"{nameof(FetchSessionList)} failed: {e.ToFancyString()}");
                throw new OfflineException("Failed to List Sessions");
            }
            string? responseString = null;
            SessionView[]? sva = null;
            try
            {
                responseString = await response.Content.ReadAsStringAsync(); //TODO find async variant?
                sva = JsonSerializer.Deserialize<SessionView[]>(responseString, JsonOptions);

            }
            catch (Exception e)
            {
                Logger.LogWarning($"{nameof(FetchSessionList)} Deserializing: {responseString}");
                Logger.LogWarning($"{nameof(FetchSessionList)} failed: {e.ToFancyString()}");
                throw new MumbleException("Failed to List Sessions");
            }
            if (!(sva.Length > 0))
                throw new MumbleException("Failed to List Sessions");
            return sva;
        }
        public async Task<ExtendedSessionView> FetchSessionAsync(long sessionId, CancellationToken token)
        {
            var url = $"{Scheme}://{Address}:{Port}/api/gamesession/getinfo";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("id" , sessionId.ToString()),
                })
            };
            request.Headers.Add("Accept", "application/x-www-form-urlencoded");
            request.Headers.Add("User-Agent", UserAgent);
            request.Headers.Add("Cookie", Cookies);
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.SendAsync(request, token);
                Logger.LogInformation($"{url} returned {response.StatusCode}");
            }
            catch (Exception e)
            {
                Logger.LogWarning($"{User.Username} failed: {e.ToFancyString()}");
                throw new OfflineException("Get session info failed");
            }
            var responseString = await response.Content.ReadAsStringAsync(); //TODO find async variant?
            var esv = JsonSerializer.Deserialize<ExtendedSessionView>(responseString, JsonOptions);
            if (esv.Id <= 0 || esv.Name == null || esv.OwnerName == null || esv.Notes == null)
                throw new MumbleException("Get session info failed");
            return esv;
        }
        public async Task AddUserToSessionAsync(long sessionId, string username, CancellationToken token)
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
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.SendAsync(request, token);
                Logger.LogInformation($"{url} returned {response.StatusCode}");
            }
            catch (Exception e)
            {
                Logger.LogWarning($"{User.Username} failed: {e.ToFancyString()}");
                throw new OfflineException("Get session info failed");
            }
            if (!response.IsSuccessStatusCode)
                throw new MumbleException("adduser failed");
        }


        private bool IsValidUuid(string uuid)
        {
            if (uuid.Length != 512) return false;
            return true;
        }
    }
}
