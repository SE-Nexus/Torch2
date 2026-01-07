using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Torch2API.Constants;
using Torch2API.DTOs.Instances;
using Torch2API.DTOs.Logs;
using Torch2API.Models.Configs;
using Torch2WebUI.Models;
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
        public IActionResult GetStatus([FromBody] TorchInstance status, [FromServices] InstanceManager InstanceService)
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
        public IActionResult RegisterInstance([FromBody] TorchInstance status, [FromServices] InstanceManager InstanceService)
        {
            InstanceService.RegisterInstance(status);
            // Example handling
            Console.WriteLine($"Instance Registered: {status.Name} - {status.InstanceID}");
            return Ok();
            
        }

        /// <summary>
        /// Handles a POST request to retrieve all configured instance objects provided in the request body.
        /// </summary>
        /// <param name="allinstances">A list of instance configuration objects received from the request body. Represents the set of instances to
        /// be processed.</param>
        /// <returns>An <see cref="IActionResult"/> that indicates the result of the operation.</returns>
        [HttpPost("allprofiles")]
        public IActionResult GetAllConfiguredProfiles([FromBody] List<ProfileCfg> allinstances, [FromServices] InstanceManager InstanceService)
        {
            var headers = HttpContext.Request.Headers;
            string? instanceid = headers[TorchConstants.InstanceIdHeader].FirstOrDefault();
            InstanceService.UpdateProfiles(instanceid, allinstances);
            return Ok();
        }



    }
}
