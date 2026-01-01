using IgniteSE1.Services;
using IgniteUtils.Commands;
using IgniteUtils.Commands.StateCommands;
using IgniteUtils.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch2API.Models;

namespace IgniteSE1.Commands
{
    public class StateCommands : ServerStateCommands
    {

        private GameService gameService;
        private ServerStateService serverStateService;

        public StateCommands(GameService service, ServerStateService serverState)
        {
            this.gameService = service;
            this.serverStateService = serverState; 



        }


        public override void Start(ICommandContext ctx)
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

        public override void Status(ICommandContext ctx)
        {
            ctx.Respond(serverStateService.ToString());
        }

        public override void Stop(ICommandContext ctx, bool kill)
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
