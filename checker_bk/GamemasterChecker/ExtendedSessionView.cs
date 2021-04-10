using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Gamemaster.Models.View
{
#pragma warning disable CS8618
    public class ExtendedSessionView
    {
        public long Id { get; set; }
        public string OwnerName { get; set; }
        public DateTime Timestamp { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
    }
#pragma warning restore CS8618
}
