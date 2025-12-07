using System;
using System.Collections.Generic;
using System.Text;

namespace IgniteUtils.Attributes
{

    [AttributeUsage(AttributeTargets.Property)]
    public class EnvVarAttribute : Attribute
    {
        public string Name { get; }
        public EnvironmentVariableTarget Target { get; }


        public EnvVarAttribute(string name, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
        {
            Name = name;
            Target = target;
        }
    }
}
