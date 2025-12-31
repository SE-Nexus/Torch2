using Microsoft.AspNetCore.Mvc;
using Torch2API.DTOs.Instances;
using Torch2API.DTOs.Logs;

namespace Torch2WebUI.APIControllers.Status
{
    [ApiController]
    [Route("api/instance/status")]
    public class StatusController : ControllerBase
    {
        [HttpPost("Update")]
        public IActionResult GetStatus([FromBody] TorchInstanceBase status)
        {
            // Example handling
            Console.WriteLine($"Status Recieved: {status.Name} - {status.InstanceID}");

            return Ok();
        }
    }
}
