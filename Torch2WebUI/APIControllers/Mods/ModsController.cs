using Microsoft.AspNetCore.Mvc;
using Torch2WebUI.Models.DTOs;
using Torch2WebUI.Services.ModServices;

namespace Torch2WebUI.APIControllers.Mods
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModsController : ControllerBase
    {
        private readonly IModsService _modsService;

        public ModsController(IModsService modsService)
        {
            _modsService = modsService;
        }

        /// <summary>
        /// Get all mod lists
        /// </summary>
        [HttpGet("lists")]
        public async Task<ActionResult<List<ModListDto>>> GetAllModLists()
        {
            try
            {
                var modLists = await _modsService.GetAllModListsAsync();
                return Ok(modLists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific mod list by id
        /// </summary>
        [HttpGet("lists/{id}")]
        public async Task<ActionResult<ModListDto>> GetModListById(int id)
        {
            try
            {
                var modList = await _modsService.GetModListByIdAsync(id);
                return Ok(modList);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new mod list
        /// </summary>
        [HttpPost("lists")]
        public async Task<ActionResult<ModListDto>> CreateModList([FromBody] CreateModListRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Mod list name is required" });

            try
            {
                var modList = await _modsService.CreateModListAsync(request);
                return CreatedAtAction(nameof(GetModListById), new { id = modList.Id }, modList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing mod list
        /// </summary>
        [HttpPut("lists/{id}")]
        public async Task<IActionResult> UpdateModList(int id, [FromBody] CreateModListRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Mod list name is required" });

            try
            {
                var success = await _modsService.UpdateModListAsync(id, request);
                if (!success)
                    return NotFound(new { message = $"ModList with id {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a mod list
        /// </summary>
        [HttpDelete("lists/{id}")]
        public async Task<IActionResult> DeleteModList(int id)
        {
            try
            {
                var success = await _modsService.DeleteModListAsync(id);
                if (!success)
                    return NotFound(new { message = $"ModList with id {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Add a mod to a mod list
        /// </summary>
        [HttpPost("mods")]
        public async Task<ActionResult<ModDto>> AddMod([FromBody] AddModRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ModId) || string.IsNullOrWhiteSpace(request.ModUrl))
                return BadRequest(new { message = "ModId and ModUrl are required" });

            try
            {
                var mod = await _modsService.AddModAsync(request);
                return CreatedAtAction(nameof(GetModListById), new { id = request.ModListId }, mod);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Remove a mod from a mod list
        /// </summary>
        [HttpDelete("mods/{modId}")]
        public async Task<IActionResult> RemoveMod(int modId)
        {
            try
            {
                var success = await _modsService.RemoveModAsync(modId);
                if (!success)
                    return NotFound(new { message = $"Mod with id {modId} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update a mod
        /// </summary>
        [HttpPut("mods/{modId}")]
        public async Task<IActionResult> UpdateMod(int modId, [FromBody] AddModRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ModId) || string.IsNullOrWhiteSpace(request.ModUrl))
                return BadRequest(new { message = "ModId and ModUrl are required" });

            try
            {
                var success = await _modsService.UpdateModAsync(modId, request);
                if (!success)
                    return NotFound(new { message = $"Mod with id {modId} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
