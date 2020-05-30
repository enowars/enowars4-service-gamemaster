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
using EnoCore.Json;

namespace GamemasterChecker.Controllers
{
    [ApiController]
    [Route("/")]
    public class CheckerController : Controller
    {
        private readonly JsonSerializerOptions JsonOptions;
        private readonly ILogger<CheckerController> Logger;
        private readonly IChecker Checker;

        public CheckerController(ILogger<CheckerController> logger, IChecker checker)
        {
            Logger = logger;
            Checker = checker;
            JsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            JsonOptions.Converters.Add(new CheckerResultMessageJsonConverter());
        }

        [HttpPost]
        public async Task<IActionResult> Flag([FromBody] string content)
        {
            var result = CheckerResult.InternalError;
            var taskMessage = JsonSerializer.Deserialize<CheckerTaskMessage>(content, JsonOptions);
            try
            {
                using var scope = Logger.BeginEnoScope(taskMessage);
                result = taskMessage.Method switch
                {
                    "putflag" => await Checker.HandlePutFlag(taskMessage, HttpContext.RequestAborted),
                    "getflag" => await Checker.HandleGetFlag(taskMessage, HttpContext.RequestAborted),
                    "putnoise" => await Checker.HandlePutNoise(taskMessage, HttpContext.RequestAborted),
                    "getnoise" => await Checker.HandlePutNoise(taskMessage, HttpContext.RequestAborted),
                    "havok" => await Checker.HandleHavok(taskMessage, HttpContext.RequestAborted),
                    _ => throw new InvalidOperationException($"Invalid method {taskMessage.Method}"),
                };
                return Ok(JsonSerializer.Serialize(result, JsonOptions));
            }
            catch (Exception)
            {
                return Ok(JsonSerializer.Serialize(result, JsonOptions));
            }
        }
    }
}
