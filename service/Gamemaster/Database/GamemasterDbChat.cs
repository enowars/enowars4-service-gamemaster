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
    public partial interface IGamemasterDb
    {
        Task<ChatMessage> InsertChatMessage(long context, User sender, string content);
        Task<ChatMessageView[]> GetChatMessages(long context);
    }
    public partial class GamemasterDb : IGamemasterDb
    {
        public async Task<ChatMessage> InsertChatMessage(long context, User sender, string content)
        {
            var msg = new ChatMessage()
            {
                Sender = sender,
                SessionContextId = context,
                Content = content
            };
            await _context.ChatMessages.AddAsync(msg);
            return msg;
        }
        public async Task<ChatMessageView[]> GetChatMessages(long context)
        {
            return await _context.ChatMessages.Where(msg => msg.SessionContextId == context).OrderByDescending(msg => msg.Timestamp).Take(100).Select(m => new ChatMessageView(m)).ToArrayAsync();
        }
    }
}
