using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamemasterChecker
{
    public class MumbleException : Exception
    {
        public string ScoreboardMessage { get; set; }

        public MumbleException(string scoreboardMessage)
        {
            ScoreboardMessage = scoreboardMessage;
        }
    }

    public class OfflineException : Exception
    {
        public string ScoreboardMessage { get; set; }

        public OfflineException(string scoreboardMessage)
        {
            ScoreboardMessage = scoreboardMessage;
        }
    }
}
