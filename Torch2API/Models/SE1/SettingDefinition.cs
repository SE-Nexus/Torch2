using System;
using System.Collections.Generic;
using System.Text;

namespace Torch2API.Models.Schema
{
    public class SettingDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public object Value { get; set; } = new object();

        public string Description { get; set; } = string.Empty;

        public double? Min { get; set; }
        public double? Max { get; set; }
        
        public bool IsList { get; set; }

        public SettingDefinition() { }
    }
}
