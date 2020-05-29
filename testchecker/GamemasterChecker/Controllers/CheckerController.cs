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
            
            CheckerTask t = (CheckerTask)JsonConvert.DeserializeObject(content);
            try
            {
                CheckerResultMessage result = CheckerResultMessage.InternalError;
                switch (t.TaskType)
                {
                    case "putflag":
                        result = await Checker.HandlePutFlag(t);
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
                    default:
                        throw new InvalidOperationException($"Invalid method {t.TaskType}");
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
