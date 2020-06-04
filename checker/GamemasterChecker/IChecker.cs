using EnoCore.Models;
using EnoCore.Models.Database;
using EnoCore.Models.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GamemasterChecker
{
    public interface IChecker
    {
        Task<CheckerResult> HandlePutFlag(CheckerTaskMessage task, CancellationToken Token);
        Task<CheckerResult> HandleGetFlag(CheckerTaskMessage task, CancellationToken Token);
        Task<CheckerResult> HandlePutNoise(CheckerTaskMessage task, CancellationToken Token);
        Task<CheckerResult> HandleGetNoise(CheckerTaskMessage task, CancellationToken Token);
        Task<CheckerResult> HandleHavok(CheckerTaskMessage task, CancellationToken Token);
    }
}
