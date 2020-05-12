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

        public async Task<User> InsertUser(string name, string email, string password)
        {
            byte[] salt = new byte[16];
            byte[] passwordBytes = Encoding.Unicode.GetBytes(password);
            using var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(salt);
            using var sha512 = new SHA512Managed();
            var hash = sha512.ComputeHash(salt.Concat(passwordBytes).ToArray());

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
    }
}
