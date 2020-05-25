using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Gamemaster.Database;
using Gamemaster.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Gamemaster.Models.Database;
using SQLitePCL;
using Gamemaster.Models.View;

namespace Gamemaster.Hubs
{
    //[Authorize]
    public class SessionHub : Hub
    {
        public static Dictionary<long, Scene> Scenes = new Dictionary<long, Scene>();
        private readonly ILogger Logger;
        private readonly IGamemasterDb Db;
        public static Dictionary<string, long> ConIdtoSessionId = new Dictionary<string, long>();

        public SessionHub(ILogger<SessionHub> logger, IGamemasterDb db)
        {
            Logger = logger;
            Db = db;
        }

        public override async Task OnConnectedAsync()
        {
            string id = Context.ConnectionId;
            Logger.LogError($"OnConnectedAsync {Context.User.FindFirst(ClaimTypes.NameIdentifier).Value}  ||  {id}");
            await Task.CompletedTask;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string id = Context.ConnectionId;
            var sceneId = ConIdtoSessionId[id];
            var scene = Scenes[sceneId];
            if (scene != null)
            {
                scene.RemoveUnit("unit"+Context.ConnectionId);
                await Clients.All.SendAsync(nameof(scene), scene, CancellationToken.None);

            }
        }
        public async Task Chat(string Message)
        {
            var currentUsername = Context.User.Identity.Name;
            if (currentUsername == null) return;
            var currentUser = (await Db.GetUser(currentUsername));
            if (currentUser == null) return;
            var currentUserId = currentUser.Id;
            var sid = ConIdtoSessionId[Context.ConnectionId];
            var session = await Db.GetSession(sid, currentUserId);
            if (session == null) return;
            var msg = await Db.InsertChatMessage(session.Id, currentUser, Message);
            await Clients.Group(sid.ToString()).SendAsync("Chat", new ChatMessageView(msg), CancellationToken.None);
        }
        public async Task Join(long sid)
        {
            var currentUsername = Context.User.Identity.Name;
            if (currentUsername == null) return;
            var currentUser = (await Db.GetUser(currentUsername));
            if (currentUser == null) return;
            var currentUserId = currentUser.Id;
            var session = await Db.GetSession(sid, currentUserId);
            if (session == null) return;
            await Groups.AddToGroupAsync(Context.ConnectionId, sid.ToString());
            lock (Scenes)
            {
                if (!Scenes.ContainsKey(sid))
                {
                    Scenes[sid] = new Scene();
                }
            };
            lock (ConIdtoSessionId)
            {
                Logger.LogError($"Join, ID:::{Context.ConnectionId}");
                ConIdtoSessionId.Add(Context.ConnectionId, sid);
            }
            Scenes[sid].AddUnit("unit"+ Context.ConnectionId, new Unit());
            await Clients.Group(sid.ToString()).SendAsync("Scene", Scenes[sid], CancellationToken.None);
        }
        public async Task Move(Direction d)
        {
            var currentUsername = Context.User.Identity.Name;
            if (currentUsername == null) return;
            var currentUser = await Db.GetUser(currentUsername);
            if (currentUser == null) return;
            var currentUserId = currentUser.Id;
            Logger.LogError($"Move, ID:::{Context.ConnectionId}");
            var sid = ConIdtoSessionId[Context.ConnectionId];
            var session = await Db.GetSession(sid, currentUserId);
            if (session == null) return;
            Scenes[sid].Move("unit" + Context.ConnectionId, d);
            await Clients.Group(sid.ToString()).SendAsync("Scene", Scenes[sid], CancellationToken.None);
        }
        public async Task Drag(int x, int y)
        {
            var currentUsername = Context.User.Identity.Name;
            if (currentUsername == null) return;
            var currentUser = await Db.GetUser(currentUsername);
            if (currentUser == null) return;
            var currentUserId = currentUser.Id;
            Logger.LogError($"Move, ID:::{Context.ConnectionId}");
            var sid = ConIdtoSessionId[Context.ConnectionId];
            var session = await Db.GetSession(sid, currentUserId);
            if (session == null) return;
            Scenes[sid].Drag("unit" + Context.ConnectionId, x, y);
            await Clients.Group(sid.ToString()).SendAsync("Scene", Scenes[sid], CancellationToken.None);
        }
    }
}
