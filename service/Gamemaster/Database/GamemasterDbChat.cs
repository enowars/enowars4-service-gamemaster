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
    }
}
