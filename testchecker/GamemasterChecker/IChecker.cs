using EnoCore.Models;
using EnoCore.Models.Database;
using GamemasterChecker.Models.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamemasterChecker
{
    public interface IChecker
    {
        Task<CheckerResultMessage> HandlePutFlag(CheckerTaskMessage task);
        Task<CheckerResultMessage> HandleGetFlag(CheckerTaskMessage task);
        Task<CheckerResultMessage> HandlePutNoise(CheckerTaskMessage task);
        Task<CheckerResultMessage> HandleGetNoise(CheckerTaskMessage task);
        Task<CheckerResultMessage> HandleHavok(CheckerTaskMessage task);
    }
}
