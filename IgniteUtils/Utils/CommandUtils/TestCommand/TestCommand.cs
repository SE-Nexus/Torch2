using IgniteSE1.Services;
using InstanceUtils.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch2API.Attributes;

namespace InstanceUtils.Commands.TestCommand
{
    [CommandGroup("utils", "Simple CLI utilities")]
    public class TestCommand
    {
        private GameService gameService;

        public TestCommand(GameService service) 
        {
            this.gameService = service;
        }


        [Command("ping", "Pings the server")]
        public void Start(ICommandContext ctx)
        {
            ctx.Respond("Pong");
        }


        [Command("basedir", "Stops the server")]
        public void Stop(ICommandContext ctx)
        {
            ctx.Respond(AppDomain.CurrentDomain.BaseDirectory);
        }


        /*
        [GroupCommandActionAttribute]
        public void Execute(ICommandContext ctx)
        {
            ctx.Respond("Pong");
        }
        */

        
    }
}
