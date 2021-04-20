namespace GamemasterChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Metadata.Ecma335;
    using System.Threading;
    using System.Threading.Tasks;
    using EnoCore;
    using EnoCore.Checker;
    using GamemasterChecker.DbModels;
    using GamemasterChecker.Models;
    using Microsoft.AspNetCore.SignalR.Client;
    using Microsoft.Extensions.Logging;

    public class GamemasterSignalRClient : IAsyncDisposable
    {
        private readonly ILogger logger;
        private readonly string scheme = "http";
        private readonly int port = 8001;
        private GamemasterUser? user;
        private HubConnection? connection;
        private TaskCompletionSource<bool>? source;
        private CancellationToken token;
        private CancellationTokenRegistration? reg;
        private string? contentToCompare;

        public GamemasterSignalRClient(ILogger<GamemasterClient> logger)
        {
            this.logger = logger;
        }

        public void Start(
            string address,
            GamemasterUser user,
            TaskCompletionSource<bool>? source,
            string? contentToCompare,
            IEnumerable<string> cookies,
            CancellationToken token)
        {
            this.token = token;
            this.user = user;
            this.source = source;
            this.contentToCompare = contentToCompare;
            this.reg = this.token.Register(() =>
            {
                this.logger.LogWarning("The CancellationToken was cancelled, disposing SignalRClient");
                this.source?.TrySetException(new MumbleException("Chat messages not delivered in time"));
            });
            this.connection = new HubConnectionBuilder()
                .WithUrl(this.scheme + "://" + address + ":" + this.port + "/hubs/session", options =>
                {
                    options.Headers.Add("Cookie", cookies.FirstOrDefault());
                })
                .Build();
            this.connection.On<ChatMessageView[]>("Chat", (messages) =>
            {
                this.logger.LogInformation($"{user.Username} {this.connection.ConnectionId} ChatMessage received: {messages.Length}");
                if (this.contentToCompare != null)
                {
                    this.logger.LogDebug($"Comparing {messages.Count()} messages to: {this.contentToCompare}");
                    foreach (var e in messages)
                    {
                        if (e.Content == this.contentToCompare)
                        {
                            Task.Run(() => this.source?.SetResult(false));
                            return;
                        }
                    }
                }
                else
                {
                    source?.SetResult(false);
                }
            });
            this.connection.On<Scene>("Scene", (scene) =>
            {
                this.logger.LogInformation($"{user.Username} Received scene with {scene.Units.Count} units {token.IsCancellationRequested} {this.connection.State} {this.connection.ConnectionId}");
            });
            this.connection.Closed += this.Connection_Closed;
        }

        public async ValueTask DisposeAsync()
        {
            if (this.reg is CancellationTokenRegistration reg)
            {
                reg.Dispose();
            }

            if (this.connection != null)
            {
                await this.connection.StopAsync();
                await this.connection.DisposeAsync();
            }
        }

        public async Task Connect()
        {
            try
            {
                await this.connection!.StartAsync(this.token);
                this.logger.LogInformation($"{this.user!.Username} StartAsync succeeded: {this.connection.ConnectionId} {this.connection.State}");
            }
            catch (Exception e)
            {
                // TODO handle auth failures, raise mumble
                this.logger.LogWarning($"{this.user!.Username} StartAsync failed: {e.ToFancyString()}");
                throw new OfflineException("Connection to session hub failed");
            }
        }

        public async Task SendMessage(string msg, CancellationToken token)
        {
            try
            {
                this.logger.LogInformation($"{this.user!.Username} {this.connection!.ConnectionId} InvokeAsync(Chat) {msg}");
                await this.connection.InvokeAsync("Chat", msg, cancellationToken: token);
            }
            catch (Exception e)
            {
                this.logger.LogError(e.ToFancyString());
            }
        }

        public async Task Join(long sid, CancellationToken token)
        {
            try
            {
                this.logger.LogInformation($"{this.user!.Username} InvokeAsync(Join) " + token.IsCancellationRequested + " " + this.connection!.State + " " + this.connection.ConnectionId);
                await this.connection.InvokeAsync("Join", sid, cancellationToken: token);
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"{this.user!.Username} failed: {e.ToFancyString()}");
                throw new OfflineException("Join invocation in session hub failed");
            }
        }

        private Task Connection_Closed(Exception arg)
        {
            this.logger.LogInformation($"{this.user!.Username} Connection closed: {this.connection!.ConnectionId}, {arg.ToFancyString()}");
            this.source?.TrySetException(new OfflineException("Connection to session hub was closed by the server"));
            return Task.CompletedTask;
        }
    }
}
