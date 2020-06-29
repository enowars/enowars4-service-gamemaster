using EnoCore.Utils;
using Gamemaster.Models;
using Gamemaster.Models.View;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GamemasterChecker
{
    public class GamemasterSignalR : IAsyncDisposable
    {
        private readonly ILogger Logger;
        private readonly string Scheme = "http";
        private readonly int Port = 8001;
        private readonly GamemasterUser User;
        private readonly HubConnection connection;
        private readonly TaskCompletionSource<bool>? Source;
        private readonly CancellationToken Token;
        private readonly CancellationTokenRegistration Reg;
        private readonly string? ContentToCompare;
        private int MessageBatchIndex;
        public GamemasterSignalR(string address, GamemasterUser user, ILogger logger, TaskCompletionSource<bool>? source, int messageBatchIndex, string? contentToCompare, GamemasterClient client, CancellationToken token)
        {
            Token = token;
            Logger = logger;
            User = user;
            Source = source;
            ContentToCompare = contentToCompare;
            MessageBatchIndex = messageBatchIndex;
            Reg = Token.Register(() =>
            {
                Logger.LogWarning("The CancellationToken was cancelled, disposing SignalRClient");
                Source?.TrySetException(new MumbleException("Chat messages not delivered in time"));
            });
            connection = new HubConnectionBuilder()
                .WithUrl(Scheme + "://" + address + ":" + Port + "/hubs/session", options=>
                {
                    options.Headers.Add("Cookie", client.Cookies.FirstOrDefault());
                })
                .Build();
            connection.On<ChatMessageView[]>("Chat", (messages) =>
            {
                Logger.LogInformation($"{user.Username} {connection.ConnectionId} ChatMessage received: {messages.Length}");
                if (ContentToCompare != null)
                {
                    if (MessageBatchIndex != 0)
                        MessageBatchIndex -= 1;
                    Logger.LogDebug($"Comparing {messages.Count()} messages to: {ContentToCompare}");
                    foreach (var e in messages)
                    {
                        if (e.Content == ContentToCompare)
                        {
                            Task.Run(() => Source?.SetResult(false));
                            return;
                        }
                    }
                    Source?.SetException(new MumbleException("Flag is not in chat"));
                }
                else
                {
                    source?.SetResult(false);
                }
            });
            connection.On<Scene>("Scene", (scene) =>
            {
                Logger.LogInformation($"{user.Username} Received scene with {scene.Units.Count} units {token.IsCancellationRequested} {connection.State} {connection.ConnectionId}");
            });
            connection.Closed += Connection_Closed;
        }

        private Task Connection_Closed(Exception arg)
        {
            Logger.LogInformation($"{User.Username} Connection closed: {connection.ConnectionId}, {arg.ToFancyString()}");
            Source?.TrySetException(new OfflineException("Connection to session hub was closed by the server"));
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            Reg.Dispose();
            await connection.StopAsync();
        }

        public async Task Connect()
        {
            try
            {
                await connection.StartAsync(Token);
                Logger.LogInformation($"{User.Username} StartAsync succeeded: {connection.ConnectionId} {connection.State}");
            }
            catch(Exception e) //TODO handle auth failures, raise mumble
            {
                Logger.LogWarning($"{User.Username} StartAsync failed: {e.ToFancyString()}");
                throw new OfflineException("Connection to session hub failed");
            }
        }

        public async Task SendMessage(string msg, CancellationToken token)
        {
            try
            {
                Logger.LogInformation($"{User.Username} {connection.ConnectionId} InvokeAsync(Chat) {msg}");
                await connection.InvokeAsync("Chat", msg, cancellationToken: token);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToFancyString());
            }
        }
        public async Task Join(long sid, CancellationToken token)
        {
            try
            {
                Logger.LogInformation($"{User.Username} InvokeAsync(Join) " + token.IsCancellationRequested + " " + connection.State + " " + connection.ConnectionId);
                await connection.InvokeAsync("Join", sid, cancellationToken: token);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"{User.Username} failed: {e.ToFancyString()}");
                throw new OfflineException("Join invocation in session hub failed");
            }
        }
    }
}
