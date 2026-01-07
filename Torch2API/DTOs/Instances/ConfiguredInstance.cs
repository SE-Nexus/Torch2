using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace Torch2API.DTOs.Instances
{
    public class ConfiguredInstance
    {
        [Key]
        public string InstanceID { get; set; }

        public string Name { get; set; }

        public string MachineName { get; set; }

        public string IPAddress { get; set; }

        public int GamePort { get; set; }

        public string ProfileName { get; set; }

        public string TargetWorld { get; set; }

        public string TorchVersion { get; set; }


        [JsonIgnore]
        public DateTime LastUpdate { get; set; }



        public string GetShortId()
        {
            if (string.IsNullOrEmpty(InstanceID))
                return string.Empty;

            if (InstanceID.Length > 6)
            {
                return InstanceID.Substring(InstanceID.Length - 6, 6)
                                 .ToUpperInvariant();
            }

            return InstanceID;
        }
    }
}
