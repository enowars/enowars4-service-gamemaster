using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Gamemaster.Database;
using Gamemaster.Models.Database;

namespace Gamemaster.Controller
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GameSessionController : ControllerBase
    {
        private readonly ILogger<GameSessionController> Logger;
        private readonly IPnPAppDb Db;

        public GameSessionController(ILogger<GameSessionController> logger, IPnPAppDb db)
        {
            Logger = logger;
            Db = db;
        }
        [HttpGet]
        public async Task<ActionResult> Create(string name, string notes, string password)
        {
            try
            {
                var username = HttpContext.User.Identity.Name;
                var owner = await Db.GetUser(username);
                var session = await Db.InsertSession(name, notes, owner, password);
            }catch (Exception e)
            {
                Logger.LogError($"{nameof(Create)} failed: {e.Message}");
                return Forbid(); 
            }
            return new EmptyResult();
        }
        [HttpGet]
        public async Task<ActionResult> AddUser(int sessionid, string username)
        {
            try
            {
                var currentusername = HttpContext.User.Identity.Name;
                if (currentusername == null)
                {
                    throw new System.Exception($"User not logged in");
                }
                var currentuser = await Db.GetUser(currentusername);
                if (!(currentuser is User _))
                {
                    throw new System.Exception($"No user called {currentusername} found");
                }
                var adduser = await Db.GetUser(username);
                if (!(adduser is User _))
                {
                    throw new System.Exception($"No user called {currentusername} found");
                }
                var session = await Db.GetSession(sessionid);
                if (!(session is Session _))
                {
                    throw new System.ArgumentException("SessionId not valid");
                }
                if (session.Owner.Id != currentuser.Id)
                {
                    throw new System.Exception($"User {currentusername} not owner of session {session.Name}");
                }
                await Db.AddUserToSession(session.Id, adduser.Id);
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(AddUser)} failed: {e.Message}");
                return Forbid();
            }
            return new EmptyResult();
        }
        [HttpPost]
        public async Task<ActionResult> AddToken([FromForm]int sessionid, [FromForm] string name, [FromForm] string description, [FromForm] bool isprivate, [FromForm] IFormFile icon)
        {
            try
            {
                var currentusername = HttpContext.User.Identity.Name;
                if (currentusername == null)
                {
                    throw new System.Exception($"User not logged in");
                }
                var currentuser = await Db.GetUser(currentusername);
                if (!(currentuser is User _))
                {
                    throw new System.Exception($"No user called {currentusername} found");
                }
                var session = await Db.GetSession(sessionid);
                if (!(session is Session _))
                {
                    throw new System.ArgumentException("SessionId not valid");
                }
                if (session.Owner.Id != currentuser.Id)
                {
                    throw new System.Exception($"User {currentusername} not owner of session {session.Name}");
                }

                MemoryStream stream = new MemoryStream();
                await icon.CopyToAsync(stream);
                var iconbin = stream.ToArray();
                await Db.AddTokenToSession(session.Id, name, description, isprivate, iconbin);
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(AddUser)} failed: {e.Message}");
                return Forbid();
            }
            return new EmptyResult();
        }
    }
}