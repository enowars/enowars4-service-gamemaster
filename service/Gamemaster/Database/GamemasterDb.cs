using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gamemaster.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Gamemaster.Database
{
    public interface IPnPAppDb
    {
        void Migrate();
        Task<User> InsertUser(string name, string email, string password);
        Task<User?> AuthenticateUser(string name, string password);
        Task<User?> GetUser(int userid);
        Task<User?> GetUser(string username);
        Task<Session> InsertSession(string name, string notes, User owner, string password);
        Task<Session?> GetSession(long sessionId);
        Task AddUserToSession(long sessionId, long userId);
        Task<Token?> AddTokenToSession(long sessionId, string name, string description, bool isprivate, byte[] icon);
    }

    public partial class PnPAppDb : IPnPAppDb
    {
        public static Random Rand = new Random(); 
        private const int MAX_PASSWORD_LENGTH = 128;
        private readonly ILogger Logger;
        private readonly GamemasterDbContext _context;

        public PnPAppDb(GamemasterDbContext context, ILogger<PnPAppDb> logger)
        {
            _context = context;
            Logger = logger;
            
        }

        public void Migrate()
        {
            var pendingMigrations = _context.Database.GetPendingMigrations().Count();
            if (pendingMigrations > 0)
            {
                Logger.LogInformation($"Applying {pendingMigrations} migration(s)");
                _context.Database.Migrate();
                _context.SaveChanges();
                Logger.LogDebug($"Database migration complete");
            }
            else
            {
                Logger.LogDebug($"No pending migrations");
            }
        }

        private void Hash(string password, ReadOnlySpan<byte> salt, Span<byte> output)
        {
            Span<byte> input = stackalloc byte[salt.Length + MAX_PASSWORD_LENGTH];
            salt.CopyTo(input);
            Encoding.UTF8.GetBytes(password, input.Slice(salt.Length));
            using var sha512 = new SHA512Managed();
            sha512.TryComputeHash(input, output, out var _);
        }

        public async Task<User> InsertUser(string name, string email, string password)
        {
            byte[] salt = new byte[16];
            byte[] hash = new byte[64];
            using var rng = new RNGCryptoServiceProvider(); 
            rng.GetBytes(salt);
            Hash(password, salt, hash);
            var user = new User()
            {
                Name = name,
                Email = email,
                PasswordSalt = salt,
                PasswordSha512Hash = hash
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<User?> AuthenticateUser(string name, string password)
        {
            User? user = null;
            try
            {
                byte[] hash = new byte[64];
                user = await _context.Users.Where(u => u.Name == name).SingleOrDefaultAsync();
                if (user == null) return null;
                Hash(password, user.PasswordSalt, hash);
                if (!user.PasswordSha512Hash.SequenceEqual(hash))
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(AuthenticateUser)} failed: {e.Message}");
            }
            return user;
        }
        public async Task<User?> GetUser(int userid)
        {
            return await _context.Users.Where(u => u.Id == userid).SingleOrDefaultAsync();
        }
        public async Task<User?> GetUser(string username)
        {
            return await _context.Users.Where(u => u.Name == username).SingleOrDefaultAsync();
        }
        public async Task<Session> InsertSession (string name, string notes, User owner, string password)
        {
            byte[] hash = new byte[64];
            byte[] salt = new byte[16];
            using var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(salt);
            Hash(password, salt, hash);
            var session = new Session()
            {
                Name = name,
                Notes = notes,
                Owner = owner,
                OwnerId = owner.Id,
                PasswordSalt = salt,
                PasswordSha512Hash = hash
            };
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }
        public async Task<Session?> GetSession (long sessionId)
        {
            return await _context.Sessions.Where(s => s.Id == sessionId).SingleOrDefaultAsync();
        }
        public async Task AddUserToSession (long sessionId, long userId)
        {
            _context.SessionUserLinks.Add(new SessionUserLink()
            {
                UserId = userId,
                SessionId = sessionId
            });
            await _context.SaveChangesAsync();
        }
        public async Task<Token?> AddTokenToSession(long sessionId, string name, string description, bool isprivate, byte[] icon)
        {
            string uUID = "";
            for (;uUID.Length<128; uUID += Rand.Next().ToString("X8"));

            var token = new Token()
            {
                Name = name,
                Description = description,
                IsPrivate = isprivate,
                Icon = icon,
                UUID = uUID
            };
            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }
    }
}
