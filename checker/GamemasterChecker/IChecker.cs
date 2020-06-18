using EnoCore.Models;
using EnoCore.Models.Database;
using EnoCore.Models.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GamemasterChecker
{
    public interface IChecker
    {
        Task<CheckerResultMessage> HandlePutFlag(CheckerTaskMessage task, CancellationToken Token);
        Task<CheckerResultMessage> HandleGetFlag(CheckerTaskMessage task, CancellationToken Token);
        Task<CheckerResultMessage> HandlePutNoise(CheckerTaskMessage task, CancellationToken Token);
        Task<CheckerResultMessage> HandleGetNoise(CheckerTaskMessage task, CancellationToken Token);
        Task<CheckerResultMessage> HandleHavok(CheckerTaskMessage task, CancellationToken Token);
    }
}
