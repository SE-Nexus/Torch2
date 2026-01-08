using System;
using System.Collections.Generic;
using System.Text;

namespace Torch2API.Models.Configs
{
    public record WorldInfo
    {
        public string Name { get; set; }

        public DateTime CreatedUtc { get; set; }

        public DateTime LastUpdatedUtc { get; set; }

        public long SizeBytes { get; set; }
    }
}
