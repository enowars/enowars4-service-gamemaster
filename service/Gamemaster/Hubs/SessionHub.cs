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

namespace Gamemaster.Hubs
{
    //[Authorize]
    public class SessionHub : Hub
    {
        public static Dictionary<string, Scene> Scenes = new Dictionary<string, Scene>();
        public string ConnectionId => "user" + Context.ConnectionId;
        private readonly ILogger Logger;
        private readonly IPnPAppDb Db;

        public SessionHub(ILogger<SessionHub> logger, IPnPAppDb db)
        {
            Logger = logger;
            Db = db;
        }

        public override async Task OnConnectedAsync()
        {
            string id = ConnectionId;
            Logger.LogDebug($"OnConnectedAsync {Context.User.FindFirst(ClaimTypes.NameIdentifier).Value}");
            if (!Scenes.ContainsKey("test"))
            {
                Scenes["test"] = new Scene();
            }
            var scene = Scenes["test"];
            scene.AddUnit(id, new Unit());
            await Clients.All.SendAsync(nameof(scene), scene, CancellationToken.None);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string id = ConnectionId;
            if (!Scenes.ContainsKey("test"))
            {
                Scenes["test"] = new Scene();
            }
            var scene = Scenes["test"];
            scene.RemoveUnit(id);
            await Clients.All.SendAsync(nameof(scene), scene, CancellationToken.None);
        }

        public async Task Move(Direction d)
        {
            var id = ConnectionId;
            var scene = Scenes["test"];
            scene.Move(id, d);
            await Clients.All.SendAsync(nameof(scene), scene, CancellationToken.None);
        }
    }
}
