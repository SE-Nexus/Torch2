namespace Torch2WebUI.Models.Database
{
    public class Mod
    {
        public int Id { get; set; }
        public int ModListId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ModId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Source { get; set; } = "SteamWorkshop"; // SteamWorkshop or ModIO
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Steam metadata (cached from API)
        public string FileSize { get; set; } = string.Empty;
        public string PreviewUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long TimeCreated { get; set; }
        public long TimeUpdated { get; set; }
        public int Subscriptions { get; set; }
        public int Favorites { get; set; }
        public int Views { get; set; }
        public string Tags { get; set; } = string.Empty; // JSON array of tags
        public DateTime SteamMetadataUpdatedAt { get; set; }

        // Navigation property
        public ModList ModList { get; set; } = null!;
    }
}
