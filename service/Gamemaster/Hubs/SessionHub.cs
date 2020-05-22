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

namespace Gamemaster.Hubs
{
    //[Authorize]
    public class SessionHub : Hub
    {
        public static Dictionary<long, Scene> Scenes = new Dictionary<long, Scene>();
        private readonly ILogger Logger;
        private readonly IPnPAppDb Db;
        public static Dictionary<string, int> CiDtoScenes = new Dictionary<string, int>();

        public SessionHub(ILogger<SessionHub> logger, IPnPAppDb db)
        {
            Logger = logger;
            Db = db;
        }

        public override async Task OnConnectedAsync()
        {
            string id = Context.ConnectionId;
            Logger.LogDebug($"OnConnectedAsync {Context.User.FindFirst(ClaimTypes.NameIdentifier).Value}");
            await Task.CompletedTask;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string id = Context.ConnectionId;
            var sceneId = CiDtoScenes[id];
            var scene = Scenes[sceneId];
            if (scene != null)
            {
                scene.RemoveUnit("unit"+Context.ConnectionId);
                await Clients.All.SendAsync(nameof(scene), scene, CancellationToken.None);

            }
        }

        public async Task Join(long id)
        {
            var currentUsername = Context.User.Identity.Name;
            if (currentUsername == null) return;
            var currentUser = (await Db.GetUser(currentUsername));
            if (currentUser == null) return;
            var currentUserId = currentUser.Id;
            var session = await Db.GetSession(id, currentUserId);
            if (session == null) return;
            await Groups.AddToGroupAsync(Context.ConnectionId, id.ToString());
            lock (Scenes)
            {
                if (!Scenes.ContainsKey(id))
                {
                    Scenes[id] = new Scene();
                }
            };
            var scene = Scenes[id];
            scene.AddUnit("unit"+ Context.ConnectionId, new Unit());
            await Clients.All.SendAsync(nameof(scene), scene, CancellationToken.None);
        }
        public async Task Move(Direction d, long id)
        {
            var currentUsername = Context.User.Identity.Name;
            if (currentUsername == null) return;
            var currentUser = (await Db.GetUser(currentUsername));
            if (currentUser == null) return;
            var currentUserId = currentUser.Id;
            var session = await Db.GetSession(id, currentUserId);
            if (session == null) return;

            var cid = Context.ConnectionId;
            var scene = Scenes[id];
            scene.Move(cid, d);
            await Clients.All.SendAsync(nameof(scene), scene, CancellationToken.None);
        }
    }
}
