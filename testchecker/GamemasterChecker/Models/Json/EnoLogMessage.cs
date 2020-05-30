using EnoCore.Models.Database;
using GamemasterChecker.Models.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EnoCore.Models.Json
{
    public class EnoLogMessage
    {
        public string? Tool { get; set; }
        public string? Type { get; set; } = "infrastructure";
        public string? Severity { get; set; }
        public string? Timestamp { get; set; }
        public string? Module { get; set; }
        public string? Function { get; set; }
        public string? Flag { get; set; }
        public long? FlagIndex { get; set; }
        public long? RunId { get; set; }
        public long? RoundId { get; set; }
        public long? RelatedRoundId { get; set; }
        public string? Message { get; set; }
        public string? TeamName { get; set; }
        public string? ServiceName { get; set; }
        public string? Method { get; set; }


        public void FromCheckerTask(CheckerTask task)
        {
            Flag = task.Payload;
            RoundId = task.CurrentRoundId;
            RelatedRoundId = task.RelatedRoundId;
            TeamName = task.TeamName;
            RunId = task.Id;
            FlagIndex = task.TaskIndex;
            ServiceName = task.ServiceName;
            Method = task.TaskType;
        }

        public void FromCheckerTask(CheckerTaskMessage task)
        {
            Flag = task.Flag;
            RoundId = task.Round;
            RelatedRoundId = task.RelatedRoundId;
            TeamName = task.Team;
            RunId = task.RunId;
            FlagIndex = task.FlagIndex;
            ServiceName = task.ServiceName;
            Method = task.Method;
        }

        public void FromCheckerTaskMessage(CheckerTaskMessage taskMessage)
        {
            Flag = taskMessage.Flag;
            RoundId = taskMessage.Round;
            RelatedRoundId = taskMessage.RelatedRoundId;
            TeamName = taskMessage.Team;
            RunId = taskMessage.RunId;
            FlagIndex = taskMessage.FlagIndex;
            ServiceName = taskMessage.ServiceName;
            Method = taskMessage.Method;
        }
    }
}
