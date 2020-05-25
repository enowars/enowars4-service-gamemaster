﻿using Microsoft.EntityFrameworkCore;
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
        public async Task<Session[]> GetSessions(long userId)
        {
            return await _context.SessionUserLinks
                .Where(sul => sul.UserId == userId)
                .Include(sul => sul.Session)
                .Select(sul => sul.Session)
                .ToArrayAsync();
        }
        public async Task<SessionView[]> GetRecentSessions(int skip, int take)
        {
            return await _context.Sessions.Include(s => s.Owner).OrderByDescending(s => s.Id).Skip(skip).Take(take).Select(s => new SessionView(s)).ToArrayAsync();
        }
        public async Task<SessionView> InsertSession(string name, string notes, User owner, string password)
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
                OwnerId = owner.Id,
                PasswordSalt = salt,
                PasswordSha512Hash = hash,
                Timestamp = DateTime.UtcNow
            };
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();
            return new SessionView(session);
        }
        public async Task<SessionView?> GetSession(long sessionId, long userId)
        {
            var session = await _context.Sessions
                .Where(s => s.Id == sessionId)
                .Include(s => s.Players)
                .SingleOrDefaultAsync();

            if (session.OwnerId == userId) return new SessionView(session);
            foreach (var u in session.Players)
            {
                if (u.UserId == userId)
                    return new SessionView(session);
            }
            return null;
        }
        public async Task<Session?> GetSession(long sessionId)     // #################Todo: Check Auth??
        {
            return await _context.Sessions.Where(s => s.Id == sessionId)
                .SingleOrDefaultAsync();
        }
        public async Task AddUserToSession(long sessionId, long userId)
        {
            _context.SessionUserLinks.Add(new SessionUserLink()
            {
                UserId = userId,
                SessionId = sessionId
            });
            await _context.SaveChangesAsync();
        }
    }
}
