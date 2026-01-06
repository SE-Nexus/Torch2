using IgniteSE1.Services;
using InstanceUtils.Commands;
using InstanceUtils.Commands.StateCommands;
using InstanceUtils.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch2API.Models;
using Torch2API.Models.Commands;

namespace IgniteSE1.Commands
{
    /// <summary>
    /// Provides server state management command implementations, including starting, stopping, and reporting the status
    /// of the server.
    /// </summary>
    /// <remarks>This class extends <see cref="ServerStateCommands"/> to handle server state transitions in
    /// response to command requests. It relies on <see cref="GameService"/> and <see cref="ServerStateService"/> to
    /// perform operations and report status. Instances of this class are typically used in server command handling
    /// scenarios to control server lifecycle and state reporting.</remarks>
    public class StateCommands : ServerStateCommands
    {

        private GameService gameService;
        private ServerStateService serverStateService;
        private ICommandContext ctx;

        public StateCommands(GameService service, ServerStateService serverState, ICommandContext ctx)
        {
            this.gameService = service;
            this.serverStateService = serverState; 
            this.ctx = ctx;
        }


        public override void Start()
        {
            if (serverStateService.RequestServerStateChange(ServerStateCommand.Start))
            {
                ctx.Respond("Server is now starting!");
            }
            else
            {
                ctx.Respond("Unable to start the server!");
            }
        }

        public override void Status()
        {
            ctx.Respond(serverStateService.ToString());
        }

        public override void Stop(bool kill)
        {
            if (serverStateService.RequestServerStateChange(ServerStateCommand.Stop))
            {
                ctx.Respond("Server is now stopping!");
            }
            else
            {
                ctx.Respond("Unable to stop the server!");
            }
        }


    }
}
