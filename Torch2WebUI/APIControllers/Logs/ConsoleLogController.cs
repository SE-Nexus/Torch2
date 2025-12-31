using Microsoft.AspNetCore.Mvc;
using Torch2API.DTOs.Logs;

namespace Torch2WebUI.APIControllers.Logs
{
    [ApiController]
    [Route("api/instance/logstream")]
    public class ConsoleLogController : ControllerBase
    {

        [HttpPost]
        public IActionResult GetLogStream([FromBody] LogLine log)
        {
            // Example handling
            Console.WriteLine($"[{log.Timestamp:u}] [{log.Level}] [{log.InstanceName}] {log.Message}");

            return Ok();
        }
    }
}
