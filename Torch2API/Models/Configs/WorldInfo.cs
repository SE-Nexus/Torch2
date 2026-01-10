using System;
using System.Collections.Generic;
using System.Text;

namespace Torch2API.Models.Configs
{
    //Used for saved worlds and scenarios
    public record WorldInfo
    {
        public string Name { get; set; }

        public DateTime CreatedUtc { get; set; }

        public DateTime LastUpdatedUtc { get; set; }

        public long SizeBytes { get; set; }



        public string? Description { get; set; }
        public string? ScenarioName { get; set; }
        public string? Briefing { get; set; }
        public bool IsCorrupted { get; set; }
        public bool HasPlanets { get; set; }
        public bool IsCampaign { get; set; }

        public string? SessionDirectoryPath { get; set; }


        //public IFormFile? PreviewImage { get; set; }
    }
}
