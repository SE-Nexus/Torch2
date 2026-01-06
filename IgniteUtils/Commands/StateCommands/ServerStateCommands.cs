using System;
using System.Collections.Generic;
using System.Text;
using Torch2API.Attributes;
using Torch2API.Models.Commands;

namespace InstanceUtils.Commands.StateCommands
{
    [CommandGroup("server", "Manage server state changes and status", CommandTypeEnum.AdminOnly)]
    public abstract class ServerStateCommands
    {

        [Command("start", "Starts the server")]
        public virtual void Start() { throw new NotImplementedException(); }

        [Command("stop", "Stops the server")]
        public virtual void Stop([Option("--kill", "Attempts to kill the process")] bool kill = false) { throw new NotImplementedException(); }

        [Command("status", "Gets the server Status")]
        public virtual void Status() { throw new NotImplementedException(); }



    }
}
