using System;
using System.Collections.Generic;
using System.Text;
using Torch2API.Attributes;

namespace InstanceUtils.Commands.StateCommands
{
    [CommandGroup("server", "Manage server state changes and status")]
    public abstract class ServerStateCommands
    {

        [Command("start", "Starts the server")]
        public virtual void Start(ICommandContext ctx) { throw new NotImplementedException(); }

        [Command("stop", "Stops the server")]
        public virtual void Stop(ICommandContext ctx, [Option("--kill", "Attempts to kill the process")] bool kill = false) { throw new NotImplementedException(); }

        [Command("status", "Gets the server Status")]
        public virtual void Status(ICommandContext ctx) { throw new NotImplementedException(); }



    }
}
