using System;

namespace Torch2API.DTOs.Chat
{
    public class ChatMessage
    {
        public ulong SteamId { get; set; }
        public string DisplayName { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string Channel { get; set; }
        public string? Target { get; set; }
    }
}
