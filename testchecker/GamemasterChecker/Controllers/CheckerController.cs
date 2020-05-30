using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using EnoCore.Models;
using EnoCore.Models.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EnoCore;
using GamemasterChecker.Models.Json;

namespace GamemasterChecker.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CheckerController : ControllerBase
    {
        private readonly ILogger<CheckerController> Logger;
        private readonly IChecker Checker;

        public CheckerController(ILogger<CheckerController> logger, IChecker checker)
        {
            Logger = logger;
            Checker = checker;
        }

        [HttpPost]
        public async Task<IActionResult> Flag([FromBody] string content)
        {
            Logger.LogInformation("###");
            return Ok();
            /*
            var taskMessage = JsonSerializer.Deserialize<CheckerTaskMessage>(content);
            try
            {
                using var scope = Logger.BeginEnoScope(taskMessage);
                CheckerResultMessage result = CheckerResultMessage.InternalError;
                switch (taskMessage.Method)
                {
                    case "putflag":
                        result = await Checker.HandlePutFlag(taskMessage);
                        break;
                    case "getflag":
                        await Checker.HandleGetFlag(taskMessage);
                        break;
                    case "putnoise":
                        await Checker.HandlePutNoise(taskMessage);
                        break;
                    case "getnoise":
                        await Checker.HandlePutNoise(taskMessage);
                        break;
                    case "havok":
                        await Checker.HandleHavok(taskMessage);
                        break;
                    default:
                        throw new InvalidOperationException($"Invalid method {taskMessage.Method}");
                }
                var str = JsonSerializer.Serialize(result);
                return Ok(str);
            }
            catch (Exception)
            {
                return Json(new { result = "INTERNAL_ERROR" });
            }
            */
        }
    }
}
