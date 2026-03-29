using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Torch2API.Constants;
using Torch2WebUI.Services.SQL;

namespace Torch2WebUI.APIControllers.ModList
{
    [ApiController]
    public class ModListController : ControllerBase
    {
        /// <summary>
        /// Gets all available mod lists with name and mod count.
        /// </summary>
        [HttpGet("api/modlist/all")]
        public async Task<IActionResult> GetAllModLists([FromServices] AppDbContext dbContext)
        {
            try
            {
                var modLists = await dbContext.ModLists
                    .Select(ml => new { ml.Id, ml.Name, ModCount = ml.Mods.Count })
                    .ToListAsync();

                return Ok(modLists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving mod lists: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the mod IDs for a specified mod list name.
        /// </summary>
        [HttpGet(WebAPIConstants.GetModIdsByListName)]
        public async Task<IActionResult> GetModIdsByListName(string name, [FromServices] AppDbContext dbContext)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Mod list name cannot be empty.");

            try
            {
                var modList = await dbContext.ModLists
                    .Where(ml => ml.Name == name)
                    .Include(ml => ml.Mods)
                    .FirstOrDefaultAsync();

                if (modList == null)
                    return NotFound($"Mod list '{name}' not found.");

                var modIds = modList.Mods.Select(m => m.ModId).ToList();
                return Ok(new { modListName = name, modIds, count = modIds.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving mod IDs: {ex.Message}");
            }
        }
    }
}
