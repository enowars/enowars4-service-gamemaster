using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnPApp.Models.Database
{
#pragma warning disable CS8618
    public class Session
    {
        public long Id { get; set; }
        public long OwnerId { get; set; }
        public User Owner { get; set; }
        public List<SessionUserLink> Players { get; set; } = new List<SessionUserLink>();
    }
#pragma warning restore CS8618
}
