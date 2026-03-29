using Microsoft.AspNetCore.Mvc;
using Torch2API.Constants;
using Torch2API.DTOs.Chat;
using Torch2WebUI.Services.InstanceServices;

namespace Torch2WebUI.APIControllers.Chat
{
    [ApiController]
    public class ChatController : ControllerBase
    {
        [HttpPost(WebAPIConstants.PostChat)]
        public IActionResult PostMessage(
            [FromBody] ChatMessage message,
            [FromServices] InstanceChatService chatService)
        {
            if (!Request.Headers.TryGetValue(TorchConstants.InstanceIdHeader, out var instanceId))
                return BadRequest("Missing Instance-Id header");

            chatService.Append(instanceId.ToString(), message);
            return Ok();
        }
    }
}
