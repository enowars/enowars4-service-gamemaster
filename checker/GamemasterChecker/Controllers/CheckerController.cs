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
using EnoCore.Utils;
using EnoCore.Models.Json;

namespace GamemasterChecker.Controllers
{
    [ApiController]
    [Route("/")]
    [Route("/service")]
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
            //JsonOptions.Converters.Add(new CheckerResultMessageJsonConverter());
        }
        [HttpGet]
        [Route("/service")]
        public async Task<IActionResult> Service()
        {
            return Ok(JsonSerializer.Serialize(new CheckerInfoMessage()
            {
                ServiceName = "Gamemaster",
                FlagCount = 3,
                NoiseCount = 0,
                HavocCount = 1
            }, JsonOptions));
        }
        [HttpPost]
        [Route("/")]
        public async Task<IActionResult> Flag([FromBody] string content)
        {
            CheckerResultMessage result = new CheckerResultMessage()
            {
                Result = CheckerResult.INTERNAL_ERROR,
                Message = null
            };
            var taskMessage = JsonSerializer.Deserialize<CheckerTaskMessage>(content, JsonOptions);
            using var scope = Logger.BeginEnoScope(taskMessage);
            Logger.LogDebug($"{nameof(CheckerController)} start handling task");
            try
            {
                switch (taskMessage.Method)
                {
                    case CheckerTaskMethod.putflag:
                        await Checker.HandlePutFlag(taskMessage, HttpContext.RequestAborted);
                        break;
                    case CheckerTaskMethod.getflag:
                        await Checker.HandleGetFlag(taskMessage, HttpContext.RequestAborted);
                        break;
                    case CheckerTaskMethod.putnoise:
                        await Checker.HandlePutNoise(taskMessage, HttpContext.RequestAborted);
                        break;
                    case CheckerTaskMethod.getnoise:
                        await Checker.HandleGetNoise(taskMessage, HttpContext.RequestAborted);
                        break;
                    case CheckerTaskMethod.havoc:
                        await Checker.HandleHavoc(taskMessage, HttpContext.RequestAborted);
                        break;
                    default:
                        throw new InvalidOperationException($"Invalid method {taskMessage.Method}");
                };
                var json = JsonSerializer.Serialize(new CheckerResultMessage()
                {
                    Result = CheckerResult.OK
                }, JsonOptions);
                Logger.LogInformation($"CheckerResultMessage OK");
                return Ok(json);
            }
            catch (MumbleException e)
            {
                Logger.LogInformation($"CheckerResultMessage MUMBLE {e.ToFancyString()}");
                return Ok(JsonSerializer.Serialize(new CheckerResultMessage()
                {
                    Result = CheckerResult.MUMBLE,
                    Message = e.ScoreboardMessage
                }));
            }
            catch (OfflineException e)
            {
                Logger.LogInformation($"CheckerResultMessage OFFLINE {e.ToFancyString()}");
                return Ok(JsonSerializer.Serialize(new CheckerResultMessage()
                {
                    Result = CheckerResult.OFFLINE,
                    Message = e.ScoreboardMessage
                }));
            }
            catch (Exception e)
            {
                Logger.LogInformation($"CheckerResultMessage INTERNAL_ERROR");
                Logger.LogError(e.ToFancyString());
                return Ok(JsonSerializer.Serialize(result, JsonOptions));
            }
        }
    }
}
