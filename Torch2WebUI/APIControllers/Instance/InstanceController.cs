using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Torch2API.DTOs.Instances;
using Torch2API.DTOs.Logs;
using Torch2WebUI.Services.InstanceServices;

namespace Torch2WebUI.APIControllers.Status
{
    [ApiController]
    [Route("api/instance")]
    public class InstanceController : ControllerBase
    {
        /// <summary>
        /// Once an instance is registered, it will begin sending status updates
        /// </summary>
        /// <param name="status"></param>
        /// <param name="InstanceService"></param>
        /// <returns></returns>
        [HttpPost("Update")]
        public IActionResult GetStatus([FromBody] TorchInstanceBase status, [FromServices] InstanceManager InstanceService)
        {

            InstanceService.UpdateStatus(status);

            // Example handling
            //Console.WriteLine($"Status Recieved: {status.Name} - {status.InstanceID}");

            return Ok();
        }


        /// <summary>
        /// Instances will continue to call register until they are acknowledged
        /// </summary>
        /// <param name="status"></param>
        /// <param name="InstanceService"></param>
        /// <returns></returns>
        [HttpPost("Regsiter")]
        public IActionResult RegisterInstance([FromBody] TorchInstanceBase status, [FromServices] InstanceManager InstanceService)
        {
            InstanceService.RegisterInstance(status);
            // Example handling
            Console.WriteLine($"Instance Registered: {status.Name} - {status.InstanceID}");
            return Ok();
            
        }
    }
}
