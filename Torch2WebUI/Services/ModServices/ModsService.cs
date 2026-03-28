using Torch2WebUI.Models.Database;
using Torch2WebUI.Models.DTOs;
using Torch2WebUI.Services.SQL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Torch2WebUI.Services.ModServices
{
    public interface IModsService
    {
        Task<List<ModListDto>> GetAllModListsAsync();
        Task<ModListDto> GetModListByIdAsync(int id);
        Task<ModListDto> GetModListByNameAsync(string name);
        Task<ModListDto> CreateModListAsync(CreateModListRequest request);
        Task<bool> DeleteModListAsync(int id);
        Task<bool> UpdateModListAsync(int id, CreateModListRequest request);
        Task<ModDto> AddModAsync(AddModRequest request);
        Task<bool> RemoveModAsync(int modId);
        Task<bool> UpdateModAsync(int modId, AddModRequest request);
        Task<bool> RefreshSteamMetadataAsync(int modListId);
    }

    public class ModsService : IModsService
    {
        private readonly AppDbContext _dbContext;
        private readonly ISteamService _steamService;
        private const int MetadataCacheHours = 24; // Refresh metadata every 24 hours

        public ModsService(AppDbContext dbContext, ISteamService steamService)
        {
            _dbContext = dbContext;
            _steamService = steamService;
        }

        public async Task<List<ModListDto>> GetAllModListsAsync()
        {
            return await _dbContext.ModLists
                .Include(ml => ml.Mods)
                .OrderByDescending(ml => ml.UpdatedAt)
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(MapToDto).ToList());
        }

        public async Task<ModListDto> GetModListByIdAsync(int id)
        {
            var modList = await _dbContext.ModLists
                .Include(ml => ml.Mods)
                .FirstOrDefaultAsync(ml => ml.Id == id);

            if (modList == null)
                throw new KeyNotFoundException($"ModList with id {id} not found");

            return MapToDto(modList);
        }

        public async Task<ModListDto> GetModListByNameAsync(string name)
        {
            var modList = await _dbContext.ModLists
                .Include(ml => ml.Mods)
                .FirstOrDefaultAsync(ml => ml.Name == name);

            if (modList == null)
                throw new KeyNotFoundException($"ModList with name {name} not found");

            return MapToDto(modList);
        }

        public async Task<ModListDto> CreateModListAsync(CreateModListRequest request)
        {
            var modList = new ModList
            {
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.ModLists.Add(modList);
            await _dbContext.SaveChangesAsync();

            return MapToDto(modList);
        }

        public async Task<bool> DeleteModListAsync(int id)
        {
            var modList = await _dbContext.ModLists.FindAsync(id);
            if (modList == null)
                return false;

            _dbContext.ModLists.Remove(modList);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateModListAsync(int id, CreateModListRequest request)
        {
            var modList = await _dbContext.ModLists.FindAsync(id);
            if (modList == null)
                return false;

            modList.Name = request.Name;
            modList.Description = request.Description;
            modList.UpdatedAt = DateTime.UtcNow;

            _dbContext.ModLists.Update(modList);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<ModDto> AddModAsync(AddModRequest request)
        {
            var modList = await _dbContext.ModLists.FindAsync(request.ModListId);
            if (modList == null)
                throw new KeyNotFoundException($"ModList with id {request.ModListId} not found");

            // Check if mod with this ID already exists in the list
            var existingMod = await _dbContext.Mods
                .FirstOrDefaultAsync(m => m.ModListId == request.ModListId && m.ModId == request.ModId);

            if (existingMod != null)
                throw new InvalidOperationException($"Mod with ID {request.ModId} already exists in this mod list");

            // Extract mod name from URL if possible
            string modName = ExtractModName(request.ModUrl) ?? $"Mod-{request.ModId}";

            var mod = new Mod
            {
                ModListId = request.ModListId,
                Name = modName,
                ModId = request.ModId,
                Url = request.ModUrl,
                Source = request.Source,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Mods.Add(mod);

            // Fetch and cache Steam metadata if it's a Steam mod
            if (request.Source == "SteamWorkshop")
            {
                await CacheSteamMetadataAsync(mod);
            }

            // Update the parent ModList's UpdatedAt
            modList.UpdatedAt = DateTime.UtcNow;
            _dbContext.ModLists.Update(modList);

            await _dbContext.SaveChangesAsync();

            return MapModToDto(mod);
        }

        public async Task<bool> RemoveModAsync(int modId)
        {
            var mod = await _dbContext.Mods.Include(m => m.ModList).FirstOrDefaultAsync(m => m.Id == modId);
            if (mod == null)
                return false;

            var modList = mod.ModList;
            _dbContext.Mods.Remove(mod);
            
            if (modList != null)
            {
                modList.UpdatedAt = DateTime.UtcNow;
                _dbContext.ModLists.Update(modList);
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateModAsync(int modId, AddModRequest request)
        {
            var mod = await _dbContext.Mods.Include(m => m.ModList).FirstOrDefaultAsync(m => m.Id == modId);
            if (mod == null)
                return false;

            string modName = ExtractModName(request.ModUrl) ?? $"Mod-{request.ModId}";

            mod.Name = modName;
            mod.ModId = request.ModId;
            mod.Url = request.ModUrl;
            mod.Source = request.Source;

            _dbContext.Mods.Update(mod);
            
            if (mod.ModList != null)
            {
                mod.ModList.UpdatedAt = DateTime.UtcNow;
                _dbContext.ModLists.Update(mod.ModList);
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        private string? ExtractModName(string url)
        {
            try
            {
                // Extract from Steam Workshop URL: https://steamcommunity.com/sharedfiles/filedetails/?id=MODID
                if (url.Contains("steamcommunity.com"))
                {
                    var uri = new Uri(url);
                    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    var id = query["id"];
                    return string.IsNullOrEmpty(id) ? null : $"Steam-{id}";
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> RefreshSteamMetadataAsync(int modListId)
        {
            try
            {
                var mods = await _dbContext.Mods
                    .Where(m => m.ModListId == modListId && m.Source == "SteamWorkshop")
                    .ToListAsync();

                if (!mods.Any())
                    return true;

                var modIds = mods.Select(m => m.ModId).ToArray();
                var steamDetails = await _steamService.GetPublishedFileDetailsAsync(modIds);

                foreach (var mod in mods)
                {
                    if (steamDetails.TryGetValue(mod.ModId, out var details))
                    {
                        ApplySteamMetadataToMod(mod, details);
                        // Explicitly mark as modified
                        _dbContext.Mods.Update(mod);
                    }
                }

                var changeCount = await _dbContext.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"RefreshSteamMetadataAsync: Updated {changeCount} mods in list {modListId}");
                return changeCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing metadata: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private async Task CacheSteamMetadataAsync(Mod mod)
        {
            if (mod.Source != "SteamWorkshop")
                return;

            try
            {
                var steamDetails = await _steamService.GetPublishedFileDetailsAsync(mod.ModId);

                if (steamDetails.TryGetValue(mod.ModId, out var details))
                {
                    ApplySteamMetadataToMod(mod, details);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to cache Steam metadata for mod {mod.ModId}: {ex.Message}");
                // Continue without metadata - this is not a critical failure
            }
        }

        private void ApplySteamMetadataToMod(Mod mod, SteamPublishedFileDetails details)
        {
            mod.FileSize = details.FileSize;
            mod.PreviewUrl = details.PreviewUrl;
            mod.Title = details.Title;
            mod.Description = details.Description;
            mod.TimeCreated = details.TimeCreated;
            mod.TimeUpdated = details.TimeUpdated;
            mod.Subscriptions = details.Subscriptions;
            mod.Favorites = details.Favorites;
            mod.Views = details.Views;
            mod.Tags = SerializeTags(details.Tags);
            mod.SteamMetadataUpdatedAt = DateTime.UtcNow;
        }

        private string SerializeTags(List<SteamTag> tags)
        {
            if (!tags.Any())
                return "[]";

            return System.Text.Json.JsonSerializer.Serialize(tags.Select(t => t.Tag).ToList());
        }

        private static ModListDto MapToDto(ModList modList)
        {
            return new ModListDto
            {
                Id = modList.Id,
                Name = modList.Name,
                Description = modList.Description,
                CreatedAt = modList.CreatedAt,
                UpdatedAt = modList.UpdatedAt,
                Mods = modList.Mods.Select(MapModToDto).ToList()
            };
        }

        private static ModDto MapModToDto(Mod mod)
        {
            return new ModDto
            {
                Id = mod.Id,
                Name = mod.Name,
                ModId = mod.ModId,
                Url = mod.Url,
                Source = mod.Source,
                CreatedAt = mod.CreatedAt,
                FileSize = mod.FileSize,
                PreviewUrl = mod.PreviewUrl,
                Title = mod.Title,
                Description = mod.Description,
                TimeCreated = mod.TimeCreated,
                TimeUpdated = mod.TimeUpdated,
                Subscriptions = mod.Subscriptions,
                Favorites = mod.Favorites,
                Views = mod.Views,
                Tags = mod.Tags,
                SteamMetadataUpdatedAt = mod.SteamMetadataUpdatedAt
            };
        }
    }
}
