namespace Torch2WebUI.Models.DTOs
{
    public class AddModRequest
    {
        public int ModListId { get; set; }
        public string ModId { get; set; } = string.Empty;
        public string ModUrl { get; set; } = string.Empty;
        public string Source { get; set; } = "SteamWorkshop";
    }
}
