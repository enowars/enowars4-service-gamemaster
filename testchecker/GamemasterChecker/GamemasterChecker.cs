using EnoCore.Models;
using EnoCore.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GamemasterChecker
{
    public class GamemasterChecker : IChecker
    {
        public GamemasterChecker()
        {

        }
        public Task<CheckerResultMessage> HandleGetFlag(CheckerTask task)
        {
            HttpClient context = HttpClientFactory.Create();
            throw new NotImplementedException();
        }
        public Task<CheckerResultMessage> HandlePutFlag(CheckerTask task)
        {
            throw new NotImplementedException();
        }
        public Task<CheckerResultMessage> HandleGetNoise(CheckerTask task)
        {
            throw new NotImplementedException();
        }
        public Task<CheckerResultMessage> HandlePutNoise(CheckerTask task)
        {
            throw new NotImplementedException();
        }
        public Task<CheckerResultMessage> HandleHavok(CheckerTask task)
        {
            throw new NotImplementedException();
        }
    }
}
