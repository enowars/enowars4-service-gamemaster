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
    public partial class GamemasterDb : IGamemasterDb
    {
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
            return await _context.Users
                .Where(u => u.Name == username)
                .SingleOrDefaultAsync();
        }
        public async Task<User?> GetUserInfo(string username)
        {
            return await _context.Users
                .Where(u => u.Name == username)
                .Include(u => u.Tokens)
                .Include(sul => sul.Sessions)
                .ThenInclude(s => s.Session)
                .ThenInclude(u => u.Owner)
                .SingleOrDefaultAsync();
        }
    }
}
