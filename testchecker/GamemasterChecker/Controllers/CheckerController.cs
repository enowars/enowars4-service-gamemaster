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
    [Route("[controller]")]
    public class CheckerController : Controller
    {
        private readonly ILogger<CheckerController> Logger;
        private readonly IChecker Checker;

        public CheckerController(ILogger<CheckerController> logger, IChecker checker)
        {
            Logger = logger;
            Checker = checker;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] string content)
        {
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
                return Ok(JsonSerializer.Serialize(result));
            }
            catch (Exception)
            {
                return Json(new { result = "INTERNAL_ERROR" });
            }
        }
    }
}
