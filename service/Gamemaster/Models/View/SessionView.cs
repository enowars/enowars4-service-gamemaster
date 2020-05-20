using Gamemaster.Models.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gamemaster.Models.View
{
#pragma warning disable CS8618
    public class SessionView
    {
        public long Id { get; set; }
        public string OwnerName { get; set; }
        public string Name { get; set; }
    }
#pragma warning restore CS8618
}
