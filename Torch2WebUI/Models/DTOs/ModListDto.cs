namespace Torch2WebUI.Models.DTOs
{
    public class ModListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ModDto> Mods { get; set; } = new();
    }

    public class ModDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ModId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Source { get; set; } = "SteamWorkshop";
        public DateTime CreatedAt { get; set; }

        // Steam metadata
        public string FileSize { get; set; }
        public string PreviewUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long TimeCreated { get; set; }
        public long TimeUpdated { get; set; }
        public int Subscriptions { get; set; }
        public int Favorites { get; set; }
        public int Views { get; set; }
        public string Tags { get; set; } = string.Empty;
        public DateTime? SteamMetadataUpdatedAt { get; set; }
    }
}
