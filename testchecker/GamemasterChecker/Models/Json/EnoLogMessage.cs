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
        public long? Round { get; set; }
        public long? RelatedRound { get; set; }
        public string? Message { get; set; }
        public string? TeamName { get; set; }
        public string? ServiceName { get; set; }
        public string? Method { get; set; }


        public void FromCheckerTask(CheckerTask task)
        {
            Flag = task.Payload;
            Round = task.CurrentRoundId;
            RelatedRound = task.RelatedRoundId;
            TeamName = task.TeamName;
            RunId = task.Id;
            FlagIndex = task.TaskIndex;
            ServiceName = task.ServiceName;
            Method = task.TaskType;
        }

        public void FromCheckerTask(CheckerTaskMessage task)
        {
            Flag = task.Flag;
            Round = task.Round;
            RelatedRound = task.RelatedRoundId;
            TeamName = task.Team;
            RunId = task.RunId;
            FlagIndex = task.FlagIndex;
            ServiceName = task.ServiceName;
            Method = task.Method;
        }

        public void FromCheckerTaskMessage(CheckerTaskMessage taskMessage)
        {
            Flag = taskMessage.Flag;
            Round = taskMessage.Round;
            RelatedRound = taskMessage.RelatedRoundId;
            TeamName = taskMessage.Team;
            RunId = taskMessage.RunId;
            FlagIndex = taskMessage.FlagIndex;
            ServiceName = taskMessage.ServiceName;
            Method = taskMessage.Method;
        }
    }
}
