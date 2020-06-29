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
        Task HandlePutFlag(CheckerTaskMessage task, CancellationToken Token);
        Task HandleGetFlag(CheckerTaskMessage task, CancellationToken Token);
        Task HandlePutNoise(CheckerTaskMessage task, CancellationToken Token);
        Task HandleGetNoise(CheckerTaskMessage task, CancellationToken Token);
        Task HandleHavok(CheckerTaskMessage task, CancellationToken Token);
    }
}
