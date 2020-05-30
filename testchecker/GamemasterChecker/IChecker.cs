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
        Task<CheckerResult> HandlePutFlag(CheckerTaskMessage task);
        Task<CheckerResult> HandleGetFlag(CheckerTaskMessage task);
        Task<CheckerResult> HandlePutNoise(CheckerTaskMessage task);
        Task<CheckerResult> HandleGetNoise(CheckerTaskMessage task);
        Task<CheckerResult> HandleHavok(CheckerTaskMessage task);
    }
}
