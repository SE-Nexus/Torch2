using Microsoft.AspNetCore.Mvc;
using Torch2API.Constants;
using Torch2API.DTOs.Instances;
using Torch2API.DTOs.Logs;
using Torch2API.Models.Configs;
using Torch2API.Models.Schema;
using Torch2WebUI.Models;
using Torch2WebUI.Services.InstanceServices;
using Torch2API.Models.SE1;

namespace Torch2WebUI.APIControllers.Status
{
    [ApiController]
    public class InstanceController : ControllerBase
    {
        private string? GetInstanceIdFromHeaders() => 
            HttpContext.Request.Headers[TorchConstants.InstanceIdHeader].FirstOrDefault();

        /// <summary>
        /// Once an instance is registered, it will begin sending status updates
        /// </summary>
        /// <param name="status"></param>
        /// <param name="InstanceService"></param>
        /// <returns></returns>
        [HttpPost(WebAPIConstants.Update)]
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
        [HttpPost(WebAPIConstants.Register)]
        public IActionResult RegisterInstance([FromBody] TorchInstanceBase status, [FromServices] InstanceManager InstanceService)
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
        [HttpPost(WebAPIConstants.AllProfiles)]
        public IActionResult GetAllConfiguredProfiles([FromBody] List<ProfileCfg> allinstances, [FromServices] InstanceManager InstanceService)
        {
            InstanceService.UpdateProfiles(GetInstanceIdFromHeaders(), allinstances);
            return Ok();
        }

        [HttpPost(WebAPIConstants.AllWorlds)]
        public IActionResult GetAllWorlds([FromBody] List<WorldInfo> allWorlds, [FromServices] InstanceManager InstanceService)
        {
            InstanceService.UpdateWorlds(GetInstanceIdFromHeaders(), allWorlds);
            return Ok();
        }

        [HttpPost(WebAPIConstants.CustomWorlds)]
        public IActionResult GetAllCustomWorlds([FromBody] List<WorldInfo> allWorlds, [FromServices] InstanceManager InstanceService)
        {
            InstanceService.UpdateWorlds(GetInstanceIdFromHeaders(), allWorlds, true);
            return Ok();
        }

        // Updated: accept full ConfigDedicatedSE1 objects from instances/panel
        [HttpPost(WebAPIConstants.DedicatedSchema)]
        public IActionResult DedicatedSchema([FromBody] ConfigDedicatedSE1 config, [FromServices] InstanceManager InstanceService)
        {
            InstanceService.UpdateDedicatedConfig(GetInstanceIdFromHeaders(), config);
            return Ok();
        }
    }
}

