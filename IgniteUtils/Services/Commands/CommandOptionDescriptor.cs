using System;
using System.Collections.Generic;
using System.Text;

namespace InstanceUtils.Services.Commands
{
    public class CommandOptionDescriptor
    {
        public string Name { get; set; }
        public string ArgName { get; set; }

        public string Description { get; set; }
        public object? DefaultValue { get; set; }
        public Type OptionType { get; set; }

        public CommandOptionDescriptor(string name, string argumentName, string description, Type optionType, object? defaultValue = null)
        {
            Name = name;
            ArgName = argumentName;
            Description = description;
            OptionType = optionType;
            DefaultValue = defaultValue;
        }

        public bool HasDefaultValue => DefaultValue != null;
    }
}
