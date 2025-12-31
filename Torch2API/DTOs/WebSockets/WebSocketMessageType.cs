using System;
using System.Collections.Generic;
using System.Text;

namespace Torch2API.DTOs.WebSockets
{
    public enum WebSocketMessageType
    {
        Unknown = 0,
        ServerStatus = 1,
        ServerStatusUpdate = 2,
        PlayerJoined = 3,
        PlayerLeft = 4,
        ChatMessage = 5
    }
}
