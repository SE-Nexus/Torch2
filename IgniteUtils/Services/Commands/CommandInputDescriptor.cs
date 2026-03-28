using System;
using System.Collections.Generic;
using System.Text;

namespace InstanceUtils.Services.Commands
{
    public class CommandInputDescriptor
    {
        public string Name { get; set; }
        public string ArgName { get; set; }
        public string Description { get; set; }
        public Type InputType { get; set; }
        public bool IsRequired { get; set; }
        public object? DefaultValue { get; set; }

        public CommandInputDescriptor(string name, string argumentName, string description, Type inputType, bool isRequired = true, object? defaultValue = null)
        {
            Name = name;
            ArgName = argumentName;
            Description = description;
            InputType = inputType;
            IsRequired = isRequired;
            DefaultValue = defaultValue;
        }

        public bool HasDefaultValue => DefaultValue != DBNull.Value;
    }
}
