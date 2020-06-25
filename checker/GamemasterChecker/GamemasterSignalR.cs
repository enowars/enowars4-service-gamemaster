using EnoCore.Utils;
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
        public GamemasterSignalR(string address, GamemasterUser user, ILogger logger, TaskCompletionSource<bool>source, CancellationToken token, GamemasterClient client)
        {
            Token = token;
            reg = Token.Register(() => source.SetResult(false));
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
                Logger.LogInformation($"ChatMessage Received: {message}");
                if (message.Content == "blabla")
                {
                    source?.SetResult(true);
                    reg.Dispose();
                }
            });
        }
        public async void Dispose()
        {
            reg.Dispose();
            await connection.StopAsync();
        }
        public async Task Connect()
        {
            try
            {
                await connection.StartAsync();
                Logger.LogInformation($"Connection Started:{connection.ConnectionId}");
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToFancyString());
            }
            
        }
        public async Task SendMessage(string msg, CancellationToken token)
        {
            try
            {
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
                await connection.InvokeAsync("Chat", sid, cancellationToken: token);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToFancyString());
            }
        }
    }
}
