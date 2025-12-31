using System;
using System.Collections.Generic;
using System.Text;

namespace Torch2API.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class CommandGroupAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public CommandGroupAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
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

        public CommandAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class GroupCommandActionAttribute : Attribute, ICmdAttribute
    {
        public string Name { get; }

        public string Description { get; }

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
    }
}
