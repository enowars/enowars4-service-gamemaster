using EnoCore.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamemasterChecker.Models.Json
{
    public class CheckerTaskMessage
    {
#pragma warning disable CS8618
        public long RunId { get; set; }
        public string Method { get; set; }
        public string Address { get; set; }
        public long ServiceId { get; set; }
        public string ServiceName { get; set; }
        public long TeamId { get; set; }
        public string Team { get; set; }
        public long RelatedRoundId { get; set; }
        public long Round { get; set; }
        public string? Flag { get; set; }
        public long FlagIndex { get; set; }

        public CheckerTaskMessage() { }
#pragma warning restore CS8618

        public CheckerTaskMessage(CheckerTask task)
        {
            RunId = task.Id;
            Method = task.TaskType;
            Address = task.Address;
            ServiceId = task.ServiceId;
            ServiceName = task.ServiceName;
            TeamId = task.TeamId;
            Team = task.TeamName;
            RelatedRoundId = task.RelatedRoundId;
            Round = task.CurrentRoundId;
            Flag = task.Payload;
            FlagIndex = task.TaskIndex;
        }
    }
}
