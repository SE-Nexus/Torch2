namespace Torch2WebUI.Models.Database
{
    public class ModList
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public ICollection<Mod> Mods { get; set; } = new List<Mod>();
    }
}
