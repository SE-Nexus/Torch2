using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Utilities.Annotations
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
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

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public CommandAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, Inherited = false)]
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

        internal void SetType(Type optionType)
        {
            this.OptionType = optionType;
        }
    }


    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class GroupCommandActionAttribute : Attribute
    {

    }
}
