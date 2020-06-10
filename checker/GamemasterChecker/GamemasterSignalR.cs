﻿using EnoCore.Utils;
using Gamemaster.Models.View;
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
        public GamemasterSignalR(string address, GamemasterUser user, ILogger logger)
        {
            Logger = logger;
            User = user;
            Address = address;
            connection = new HubConnectionBuilder()
                .WithUrl(Scheme + "://" + address + ":" + Port + "/hubs/session")
                .Build();
            connection.On<ChatMessageView>("Chat", (message) =>
            {
                Logger.LogInformation($"ChatMessage Received: {message}");
            });
        }
        public async void Dispose()
        {
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
    }
}