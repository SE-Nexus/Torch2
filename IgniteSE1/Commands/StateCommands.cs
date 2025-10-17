using IgniteSE1.Services;
using IgniteUtils.Commands;
using IgniteUtils.Commands.StateCommands;
using IgniteUtils.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        }

        public override void Status(ICommandContext ctx)
        {
            ctx.Respond($"{serverStateService.CurrentServerStatus}");
        }

        public override void Stop(ICommandContext ctx, bool kill)
        {

        }


    }
}
