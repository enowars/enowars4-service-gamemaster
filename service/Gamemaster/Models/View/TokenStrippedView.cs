using Gamemaster.Models.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Gamemaster.Models.View
{
#pragma warning disable CS8618
    public class TokenStrippedView
    {
        public string UUID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPrivate { get; set; }
        public string OwnerName { get; set; }
        public TokenStrippedView(Token t)
        {
            UUID = t.UUID;
            Name = t.Name;
            Description = t.Description;
            IsPrivate = t.IsPrivate;
            OwnerName = t.Owner.Name;
        }
    }
#pragma warning disable CS8618
}
