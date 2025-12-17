using Microsoft.AspNetCore.Mvc;

namespace Torch2WebUI.APIControllers.Logs
{
    [ApiController]
    [Route("api/instance/logstream")]
    public class ConsoleLogController : ControllerBase
    {

        [HttpGet("{instanceName}")]
        public IActionResult GetLogStream(string instanceName)
        {
            // Placeholder implementation
            return Ok($"Log stream for instance: {instanceName}");
        }
    }
}
