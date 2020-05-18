using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace PnPApp.Models.Database
{
#pragma warning disable CS8618
    public class Token
    {
        public long Id { get; set; }
        [MaxLength(64)] public string UUID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPrivate { get; set; }
        public byte[] Icon { get; set; }
    }
#pragma warning restore CS8618
}