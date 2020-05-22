using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Gamemaster.Database;
using Gamemaster.Models.Database;
using System.IO;

namespace Gamemaster.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> Logger;
        private readonly IPnPAppDb Db;

        public AccountController(ILogger<AccountController> logger, IPnPAppDb db)
        {
            Logger = logger;
            Db = db;
        }

        [HttpPost]
        public async Task<ActionResult> Register([FromForm] string username, [FromForm] string email, [FromForm] string password)
        {
            var user = await Db.InsertUser(username, email, password);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity));
            return new EmptyResult();
        }
        [HttpPost]
        public async Task<ActionResult> Login([FromForm] string username, [FromForm] string password)
        {
            var dbUser = await Db.AuthenticateUser(username, password);
            if (dbUser is User user)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync( new ClaimsPrincipal(claimsIdentity));
                return new EmptyResult();
            }
            return Forbid();
        }
        [HttpPost]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return new EmptyResult();
        }

        [HttpPost]
        public async Task<ActionResult> TestLogin()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, "1")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                //AllowRefresh = <bool>,
                // Refreshing the authentication session should be allowed.

                //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                // The time at which the authentication ticket expires. A 
                // value set here overrides the ExpireTimeSpan option of 
                // CookieAuthenticationOptions set with AddCookie.

                //IsPersistent = true,
                // Whether the authentication session is persisted across 
                // multiple requests. When used with cookies, controls
                // whether the cookie's lifetime is absolute (matching the
                // lifetime of the authentication ticket) or session-based.

                //IssuedUtc = <DateTimeOffset>,
                // The time at which the authentication ticket was issued.

                //RedirectUri = <string>
                // The full path or absolute URI to be used as an http 
                // redirect response value.
            };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return new EmptyResult();
        }
        [HttpPost]
        public async Task<ActionResult> AddToken([FromForm] string name, [FromForm] string description, [FromForm] bool isprivate, [FromForm] IFormFile icon)
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
                MemoryStream stream = new MemoryStream();
                await icon.CopyToAsync(stream);
                var iconbin = stream.ToArray();
                var token = await Db.AddTokenToUser(currentuser.Id, name, description, isprivate, iconbin);
                if (token == null)
                {
                    throw new System.Exception($"Tokencreate failed");
                }
                return Json(token.UUID);
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(AddToken)} failed: {e.Message}");
                return Forbid();
            }
        }
    }
}