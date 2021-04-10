using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gamemaster.Models.View
{
#pragma warning disable CS8618
    public class ChatMessageView
    {
        public string SenderName { get; set; }
        public long SessionContextId { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
#pragma warning restore CS8618
}