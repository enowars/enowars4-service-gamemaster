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
        Task<string> HandlePutFlag(CheckerTask task);
        Task<string> HandleGetFlag(CheckerTask task);
        Task<string> HandlePutNoise(CheckerTask task);
        Task<string> HandleGetNoise(CheckerTask task);
        Task<string> HandleHavok(CheckerTask task);

    }
}
