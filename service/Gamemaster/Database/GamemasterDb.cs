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
using Gamemaster.Models.View;

namespace Gamemaster.Database
{
    public interface IGamemasterDb
    {
        void Migrate();
        Task<User> InsertUser(string name, string email, string password);
        Task<ChatMessage> InsertChatMessage(long context, User sender, string content);
        Task<User?> AuthenticateUser(string name, string password);
        Task<User?> GetUser(int userid);
        Task<User?> GetUser(string username);
        Task<User?> GetUserInfo(string username);
        Task<SessionView> InsertSession(string name, string notes, User owner, string password);
        Task<SessionView?> GetSession(long sessionId, long userId);
        Task<Session?> GetSession(long sessionId);
        Task<Session[]> GetSessions(long userId);
        Task<SessionView[]> GetRecentSessions(int skip, int take);
        Task AddUserToSession(long sessionId, long userId);
        Task<Token?> AddTokenToUser(long sessionId, string name, string description, bool isprivate, byte[] icon);
        Task<TokenStrippedView> GetTokenByUUID(string UUID);
        Task<TokenData> GetTokenDataByUUID(string UUID);
        Task<TokenStrippedView[]> GetTokens(long userid);
    }

    public partial class GamemasterDb : IGamemasterDb
    {
        public static Random Rand = new Random(); 
        private const int MAX_PASSWORD_LENGTH = 128;
        private readonly ILogger Logger;
        private readonly GamemasterDbContext _context;

        public GamemasterDb(GamemasterDbContext context, ILogger<GamemasterDb> logger)
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
            if (password.Length > MAX_PASSWORD_LENGTH) return;
            Span<byte> input = stackalloc byte[salt.Length + MAX_PASSWORD_LENGTH];
            salt.CopyTo(input);
            Encoding.UTF8.GetBytes(password, input.Slice(salt.Length));
            using var sha512 = new SHA512Managed();
            sha512.TryComputeHash(input, output, out var _);
        }



    }
}
