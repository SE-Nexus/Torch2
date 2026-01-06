using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Torch2API.Constants
{
    public static class TorchConstants
    {
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public const string InstanceIdHeader = "Instance-Id";
    }
}
