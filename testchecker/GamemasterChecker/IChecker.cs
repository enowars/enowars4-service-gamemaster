using EnoCore.Models;
using EnoCore.Models.Database;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamemasterChecker
{
    public interface IChecker
    {
        Task<CheckerResultMessage> HandlePutFlag(CheckerTask task);
        Task<CheckerResultMessage> HandleGetFlag(CheckerTask task);
        Task<CheckerResultMessage> HandlePutNoise(CheckerTask task);
        Task<CheckerResultMessage> HandleGetNoise(CheckerTask task);
        Task<CheckerResultMessage> HandleHavok(CheckerTask task);
    }
}
