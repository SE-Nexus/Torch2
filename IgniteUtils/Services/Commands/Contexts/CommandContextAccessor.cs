using System;
using System.Collections.Generic;
using System.Text;
using Torch2API.Models.Commands;

namespace InstanceUtils.Services.Commands.Contexts
{
    public class CommandContextAccessor
    {
        public ICommandContext context { get; set; }
    }
}
