using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PnPApp.Models.Database
{
#pragma warning disable CS8618
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        [MaxLength(16)] public byte[] PasswordSalt { get; set; }
        [MaxLength(64)] public byte[] PasswordSha512Hash { get; set; }
        public List<Character> Characters { get; set; } = new List<Character>();
        public List<CampaignUserLink> Campaigns { get; set; } = new List<CampaignUserLink>();
        public List<SessionUserLink> Sessions { get; set; } = new List<SessionUserLink>();
    }
#pragma warning restore CS8618
}
