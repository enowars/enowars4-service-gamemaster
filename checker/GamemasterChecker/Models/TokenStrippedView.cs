namespace GamemasterChecker.Models
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

#pragma warning disable CS8618
    public class TokenStrippedView
    {
        public string UUID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsPrivate { get; set; }

        public string OwnerName { get; set; }
    }
#pragma warning restore CS8618
}
