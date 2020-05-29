using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EnoCore.Models.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GamemasterChecker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CheckerController : ControllerBase
    {
        private readonly ILogger<CheckerController> Logger;
        private readonly IChecker Checker;
        public CheckerController(ILogger<CheckerController> logger, IChecker checker)
        {
            Logger = logger;
            Checker = checker;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromBody] string content)
        {

            CheckerTask t = (CheckerTask) JsonConvert.DeserializeObject(content);
            switch (t.TaskType)
            {
                case "putflag":
                    await Checker.HandlePutFlag(t);
                    break;
                case "getflag":
                    await Checker.HandleGetFlag(t);
                    break;
                case "putnoise":
                    await Checker.HandlePutNoise(t);
                    break;
                case "getnoise":
                    await Checker.HandlePutNoise(t);
                    break;
                case "havok":
                    await Checker.HandleHavok(t);
                    break;
            }

    }
}
