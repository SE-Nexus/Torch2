using System;
using System.Collections.Generic;
using System.Text;
using Torch2API.Models.Commands;

namespace Torch2API.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class CommandGroupAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        public CommandTypeEnum CommandTypeOverride { get; }
        public bool HasCommandTypeOverride { get; } = false;

        public CommandGroupAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the CommandGroupAttribute class with the specified group name, description,
        /// and command type override.
        /// </summary>
        /// <param name="name">The name of the command group. Cannot be null or empty.</param>
        /// <param name="description">The description of the command group. This value is optional and can be an empty string.</param>
        /// <param name="commandtypeoverride">The command type to override for this group. Defaults to CommandTypeEnum.Console if not specified.</param>
        public CommandGroupAttribute(string name, string description = "", CommandTypeEnum commandtypeoverride = CommandTypeEnum.Console)
        {
            Name = name;
            Description = description;
            CommandTypeOverride = commandtypeoverride;
            HasCommandTypeOverride = true;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, Inherited = true)]
    public class OptionAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        private Type OptionType { get; set; }



        public OptionAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
        }

        public void SetType(Type optionType)
        {
            this.OptionType = optionType;
        }
    }


    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class CommandAttribute : Attribute, ICmdAttribute
    {
        public string Name { get; }
        public string Description { get; }
        public CommandTypeEnum? CommandType { get; }

        public CommandAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
        }

        public CommandAttribute(string name, string description = "", CommandTypeEnum commandType = CommandTypeEnum.AdminOnly)
        {
            Name = "";
            Description = description;
            CommandType = commandType;
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class GroupCommandActionAttribute : Attribute, ICmdAttribute
    {
        public string Name { get; }

        public string Description { get; }

        public CommandTypeEnum? CommandType { get; }

        public GroupCommandActionAttribute(string description = "")
        {
            Name = "";
            Description = description;
        }
    }

    public interface ICmdAttribute
    {
        public string Name { get; }
        public string Description { get; }

        public CommandTypeEnum? CommandType { get; }
    }
}
