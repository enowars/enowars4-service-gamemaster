using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gamemaster.Models.Database
{
#pragma warning disable CS8618
    public class Character
    {
        public long Id { get; set; }
        public long OwnerId { get; set; }
        public User Owner { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
#pragma warning restore CS8618
}
