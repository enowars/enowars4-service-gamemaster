using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PnPApp.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PnPApp.Database
{
    public interface IPnPAppDb
    {
        void Migrate();
        Task<User> InsertUser(string name, string email, string password);
    }

    public partial class PnPAppDb : IPnPAppDb
    {
        private const int MAX_PASSWORD_LENGTH = 128;
        private readonly ILogger Logger;
        private readonly PnPAppDbContext _context;

        public PnPAppDb(PnPAppDbContext context, ILogger<PnPAppDb> logger)
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
        public async Task<bool> AuthenticateUser(string name, string password)
        {
            try
            {
                byte[] hash = new byte[64];
                var user = await _context.Users.Where(u => u.Name == name).SingleOrDefaultAsync();
                if (user == null) return false;
                Hash(password, user.PasswordSalt, hash);
                if (!user.PasswordSha512Hash.SequenceEqual(hash))
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(AuthenticateUser)} failed: {e.Message}");
            }
            return true;
        }
    }
}
