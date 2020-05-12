using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnPApp.Models.Database
{
#pragma warning disable CS8618
    public class CampaignUserLink
    {
        public long CampaignId { get; set; }
        public Campaign Campaign { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
    }
#pragma warning restore CS8618
}
