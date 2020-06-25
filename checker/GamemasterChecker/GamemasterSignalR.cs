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
    public class GamemasterSignalR : IDisposable
    {
        private readonly ILogger Logger;
        private readonly string Address;
        private readonly string Scheme = "http";
        private readonly int Port = 8001;
        private readonly GamemasterUser User;
        private readonly HubConnection connection;
        private TaskCompletionSource<bool> Source;
        private CancellationToken Token;
        private CancellationTokenRegistration reg;
        public GamemasterSignalR(string address, GamemasterUser user, ILogger logger, TaskCompletionSource<bool> source, GamemasterClient client, CancellationToken token)
        {
            Token = token;
            reg = Token.Register(() =>
            {
                Logger.LogInformation("The CancellationToken was cancelled, disposing SignalRClient");
                source.SetResult(false);
            });
            Logger = logger;
            User = user;
            Address = address;
            Source = source;
            connection = new HubConnectionBuilder()
                .WithUrl(Scheme + "://" + address + ":" + Port + "/hubs/session", options=>
                {
                    //options.Cookies.SetCookies(new Uri(Scheme + "://" + address + ":" + Port), client.Cookies);
                    options.Headers.Add("Cookie", client.Cookies.FirstOrDefault());
                })
                .Build();
            connection.On<ChatMessageView>("Chat", (message) =>
            {
                Logger.LogInformation($"ChatMessage Received: {message} " + token.IsCancellationRequested + " " + connection.State + " " + connection.ConnectionId);
                if (message.Content == "blabla")
                {
                    Task.Run(() => source?.SetResult(true));
                }
            });
            connection.On<Scene>("Scene", (scene) =>
            {
                Logger.LogInformation($"Such a scene: {scene} {token.IsCancellationRequested} {connection.State} {connection.ConnectionId}");
            });
            connection.Closed += Connection_Closed;
        }

        private Task Connection_Closed(Exception arg)
        {
            Logger.LogInformation($"Connection closed:{connection.ConnectionId}, {arg.ToFancyString()}");
            Source.SetException(new Exception($"Connection has been closed"));
            return Task.CompletedTask;
        }

        public async void Dispose()
        {
            Logger.LogInformation($"Disposing connection:{connection.ConnectionId}");
            reg.Dispose();
            await connection.StopAsync();
            Logger.LogInformation($"connection stopped:{connection.ConnectionId}");
        }

        public async Task Connect()
        {
            await connection.StartAsync(Token);
            Logger.LogInformation($"StartAsync succeeded: {connection.ConnectionId} {connection.State}");
        }

        public async Task SendMessage(string msg, CancellationToken token)
        {
            try
            {
                Logger.LogInformation("InvokeAsync(Chat) " + token.IsCancellationRequested + " " + connection.State + " " + connection.ConnectionId);
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
                Logger.LogInformation("InvokeAsync(Join) " + token.IsCancellationRequested + " " + connection.State + " " + connection.ConnectionId);
                await connection.InvokeAsync("Join", sid, cancellationToken: token);
            }
            catch (Exception e)
            {
                Logger.LogInformation("cancelrequested=" + token.IsCancellationRequested + " state=" + connection.State + " connectionid=" + connection.ConnectionId);
                Logger.LogError(e.ToFancyString());
            }
        }
    }
}
