using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Torch2API.DTOs.WebSockets
{
    public class SocketMsgEnvelope
    {
        public string Command { get; set; }

        public string? RequestId { get; set; }

        public string? UserID { get; set; }

        //data
        public JsonElement[] Payload { get; set; } = Array.Empty<JsonElement>();


        public SocketMsgEnvelope(string Command)
        {
            this.Command = Command;

        }




    }
}
