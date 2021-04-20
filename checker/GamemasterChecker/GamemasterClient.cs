namespace GamemasterChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using EnoCore;
    using EnoCore.Checker;
    using GamemasterChecker.DbModels;
    using GamemasterChecker.Models;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public sealed class GamemasterClient : IDisposable
    {
        private readonly ILogger logger;
        private readonly HttpClient httpClient;
        private readonly string userAgent = Util.GenerateUserAgent();
        private readonly string scheme = "http";
        private readonly int port = 8001;
        private readonly JsonSerializerOptions jsonOptions;
        private IEnumerable<string>? cookies;
        private GamemasterUser? user;
        private string? address;

        public GamemasterClient(IHttpClientFactory httpFactory, ILogger<GamemasterClient> logger)
        {
            this.logger = logger;
            this.httpClient = httpFactory.CreateClient("Foo");
            this.jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            logger.LogDebug($"GamemasterClient()");
        }

        public void Dispose()
        {
            this.httpClient.Dispose();
        }

        public async Task RegisterAsync(string address, GamemasterUser user, CancellationToken token)
        {
            if (this.cookies != null)
            {
                throw new InvalidOperationException("wat");
            }

            this.address = address;
            this.user = user;
            var url = $"{this.scheme}://{address}:{this.port}/api/account/register";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string?, string?>>()
                {
                    new KeyValuePair<string?, string?>("username", user.Username),
                    new KeyValuePair<string?, string?>("email", user.Email),
                    new KeyValuePair<string?, string?>("password", user.Password),
                }),
            };
            request.Headers.Add("Accept", "application/x-www-form-urlencoded");
            request.Headers.Add("User-Agent", this.userAgent);
            HttpResponseMessage response;
            try
            {
                response = await this.httpClient.SendAsync(request, token);
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"{user.Username} failed to register: {e.ToFancyString()}");
                throw new OfflineException("Registration failed");
            }

            this.logger.LogInformation($"{url} returned {response.StatusCode}");
            if (!response.IsSuccessStatusCode)
            {
                throw new MumbleException("Registration failed");
            }

            if (this.cookies != null)
            {
                throw new InvalidOperationException("User already has cookies");
            }

            var hasCookies = response.Headers.TryGetValues("Set-Cookie", out this.cookies);
            if (!hasCookies)
            {
                throw new MumbleException("Registration failed");
            }

            this.logger.LogDebug($"Registered {this.user.Username}:{this.user.Password} {this.cookies!.First()}");
        }

        public async Task LoginAsync(string address, GamemasterUser user, CancellationToken token)
        {
            this.address = address;
            this.user = user;
            this.logger.LogInformation($"Login {user.Username}:{user.Password}");
            var url = $"{this.scheme}://{address}:{this.port}/api/account/login";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string?, string?>>()
                {
                    new KeyValuePair<string?, string?>("username", user.Username),
                    new KeyValuePair<string?, string?>("password", user.Password),
                }),
            };
            request.Headers.Add("Accept", "application/x-www-form-urlencoded");
            request.Headers.Add("User-Agent", this.userAgent);
            HttpResponseMessage response;
            try
            {
                response = await this.httpClient.SendAsync(request, token);
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"{user.Username} failed to login: {e.ToFancyString()}");
                throw new OfflineException("Login failed");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new MumbleException("Login failed");
            }

            if (this.cookies != null)
            {
                throw new InvalidOperationException("User already has cookies");
            }

            var hasCookies = response.Headers.TryGetValues("Set-Cookie", out this.cookies);
            if (!hasCookies)
            {
                throw new MumbleException("Login failed");
            }
        }

        public async Task<SessionView> CreateSessionAsync(string name, string notes, string password, CancellationToken token)
        {
            this.logger.LogDebug($"CreateSessionAsync(name={name}, notes={notes}, password={password}, cookies={this.cookies!.First()})");
            if (this.cookies == null)
            {
                throw new InvalidOperationException("Not logged in");
            }

            var url = $"{this.scheme}://{this.address}:{this.port}/api/gamesession/create";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string?, string?>>()
                {
                    new KeyValuePair<string?, string?>("name", name),
                    new KeyValuePair<string?, string?>("notes", notes),
                    new KeyValuePair<string?, string?>("password", password),
                }),
            };
            request.Headers.Add("Accept", "application/x-www-form-urlencoded");
            request.Headers.Add("User-Agent", this.userAgent);
            request.Headers.Add("Cookie", this.cookies!);
            HttpResponseMessage response;
            try
            {
                response = await this.httpClient.SendAsync(request, token);
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"{this.user!.Username} failed to create session: {e.ToFancyString()}");
                throw new OfflineException("Create session failed");
            }

            this.logger.LogInformation($"{url} returned {response.StatusCode}");
            if (!response.IsSuccessStatusCode)
            {
                throw new MumbleException("Create session failed");
            }

            var responseString = await response.Content.ReadAsStringAsync(token);
            var view = JsonSerializer.Deserialize<SessionView>(responseString, this.jsonOptions)!;
            if (view.Id <= 0 || view.Name == null || view.OwnerName == null)
            {
                this.logger.LogWarning($"{this.user!.Username} create session failed: invalid view");
                throw new MumbleException("Create session failed");
            }

            return view;
        }

        public async Task<string> AddTokenAsync(string name, string description, bool isPrivate, byte[] imageData, CancellationToken token)
        {
            var url = $"{this.scheme}://{this.address}:{this.port}/api/account/addtoken";
            var imageContent = new ByteArrayContent(imageData);
            imageContent.Headers.Add("Content-Type", "image/png");
            imageContent.Headers.Add("Content-Disposition", "form-data; name=\"icon\"; filename=\"Arrow.png\"");
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new MultipartFormDataContent
                {
                    { new StringContent(name), "\"name\"" },
                    { new StringContent(description), "\"description\"" },
                    { new StringContent(isPrivate.ToString().ToLower()), "\"isPrivate\"" },
                    { imageContent, "\"icon\"", "\"Arrow.png\"" },
                },
            };
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("User-Agent", this.userAgent);
            request.Headers.Add("Cookie", this.cookies!);
            HttpResponseMessage response;
            try
            {
                response = await this.httpClient.SendAsync(request, token);
                this.logger.LogDebug($"{url} returned {response.StatusCode}");
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"{this.user!.Username} failed to add token: {e.ToFancyString()}");
                throw new OfflineException("Create token failed");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new MumbleException("Create token failed");
            }

            var responseString = await response.Content.ReadAsStringAsync(token);
            var uuid = responseString.Replace("\"", string.Empty);
            if (!IsValidUuid(uuid))
            {
                throw new MumbleException("Create token failed");
            }

            return uuid;
        }

        public async Task<TokenStrippedView> CheckTokenAsync(string address, string uuid, CancellationToken token)
        {
            var url = $"{this.scheme}://{address}:{this.port}/api/token/info"; // TODO this.address does not throw a warning but it can be null - why?
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string?, string?>>()
                {
                    new KeyValuePair<string?, string?>("UUID", uuid),
                }),
            };
            request.Headers.Add("Accept", "application/x-www-form-urlencoded");
            request.Headers.Add("User-Agent", this.userAgent);
            if (this.cookies != null)
            {
                request.Headers.Add("Cookie", this.cookies);
            }

            HttpResponseMessage response;
            try
            {
                response = await this.httpClient.SendAsync(request, token);
                this.logger.LogInformation($"{url} returned {response.StatusCode}");
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"{this.user!.Username} failed to check token: {e.ToFancyString()}");
                throw new OfflineException("Get token info failed");
            }

            TokenStrippedView? tsv;
            try
            {
                var responseString = await response.Content.ReadAsStringAsync(token);
                tsv = JsonSerializer.Deserialize<TokenStrippedView>(responseString, this.jsonOptions);
            }
            catch
            {
                throw new MumbleException("Failed to Parse Result of /api/token/info");
            }

            if (tsv == null || tsv.Name == null || tsv.OwnerName == null || tsv.UUID == null || tsv.Description == null)
            {
                throw new MumbleException("Get token info failed");
            }

            return tsv;
        }

        public async Task<SessionView[]> FetchSessionList(string address, long skip, long take, CancellationToken token)
        {
            var url = $"{this.scheme}://{address}:{this.port}/api/gamesession/listrecent"; // why doesn't this warn about null with this.address?
            UriBuilder builder = new(url);
            builder.Query = $"skip={skip}&take={take}";
            var request = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
            request.Headers.Add("Accept", "application/x-www-form-urlencoded");
            request.Headers.Add("User-Agent", this.userAgent);
            if (this.cookies != null)
            {
                request.Headers.Add("Cookie", this.cookies);
            }

            HttpResponseMessage response;
            try
            {
                response = await this.httpClient.SendAsync(request, token);
                this.logger.LogInformation($"{url} returned {response.StatusCode}");
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"{nameof(this.FetchSessionList)} failed: {e.ToFancyString()}");
                throw new OfflineException("Failed to List Sessions");
            }

            string? responseString = null;
            SessionView[]? sva = null;
            try
            {
                responseString = await response.Content.ReadAsStringAsync(token);
                sva = JsonSerializer.Deserialize<SessionView[]>(responseString, this.jsonOptions);
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"{nameof(this.FetchSessionList)} Deserializing: {responseString}");
                this.logger.LogWarning($"{nameof(this.FetchSessionList)} failed: {e.ToFancyString()}");
                throw new MumbleException("Failed to List Sessions");
            }

            if (sva == null || !(sva.Length > 0))
            {
                throw new MumbleException("Failed to List Sessions");
            }

            return sva;
        }

        public async Task<ExtendedSessionView> FetchSessionAsync(long sessionId, CancellationToken token)
        {
            var url = $"{this.scheme}://{this.address}:{this.port}/api/gamesession/getinfo";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string?, string?>>()
                {
                    new KeyValuePair<string?, string?>("id", sessionId.ToString()),
                }),
            };
            request.Headers.Add("Accept", "application/x-www-form-urlencoded");
            request.Headers.Add("User-Agent", this.userAgent);
            request.Headers.Add("Cookie", this.cookies!);
            HttpResponseMessage response;
            try
            {
                response = await this.httpClient.SendAsync(request, token);
                this.logger.LogInformation($"{url} returned {response.StatusCode}");
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"{this.user!.Username} failed to fetch session: {e.ToFancyString()}");
                throw new OfflineException("Get session info failed");
            }

            var responseString = await response.Content.ReadAsStringAsync(token);
            var esv = JsonSerializer.Deserialize<ExtendedSessionView>(responseString, this.jsonOptions);
            if (esv == null || esv.Id <= 0 || esv.Name == null || esv.OwnerName == null || esv.Notes == null)
            {
                throw new MumbleException("Get session info failed");
            }

            return esv;
        }

        public async Task AddUserToSessionAsync(long sessionId, string username, CancellationToken token)
        {
            this.logger.LogDebug($"AddUserToSessionAsync(sessionId={sessionId}, username={username}, cookie={this.cookies!.First()})");
            var url = $"{this.scheme}://{this.address}:{this.port}/api/gamesession/adduser";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string?, string?>>()
                {
                    new KeyValuePair<string?, string?>("sessionid", sessionId.ToString()),
                    new KeyValuePair<string?, string?>("username", username),
                }),
            };
            request.Headers.Add("Accept", "application/x-www-form-urlencoded");
            request.Headers.Add("User-Agent", this.userAgent);
            request.Headers.Add("Cookie", this.cookies!);
            HttpResponseMessage response;
            try
            {
                response = await this.httpClient.SendAsync(request, token);
                this.logger.LogInformation($"{url} returned {response.StatusCode}");
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"{this.user!.Username} failed to add user to session: {e.ToFancyString()}");
                throw new OfflineException("Get session info failed");
            }

            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError($"AddUserToSessionAsync failed: {await response.Content.ReadAsStringAsync(token)}");
                throw new MumbleException("adduser failed");
            }
        }

        public GamemasterSignalRClient CreateSignalRConnection(
            string address,
            TaskCompletionSource<bool>? source,
            string? contentToCompare,
            IServiceProvider serviceProvider,
            CancellationToken token)
        {
            var signalrclient = serviceProvider.GetRequiredService<GamemasterSignalRClient>();
            signalrclient.Start(
                address,
                this.user!,
                source,
                contentToCompare,
                this.cookies!,
                token);
            return signalrclient;
        }

        private static bool IsValidUuid(string uuid)
        {
            if (uuid.Length != 512)
            {
                return false;
            }

            return true;
        }
    }
}
