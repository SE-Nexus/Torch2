using IgniteSE1.Services;
using IgniteSE1.Utilities.Annotations;
using IgniteSE1.Utilities.CLI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Utilities.TestCommand
{
    [CommandGroup("test", "Manage game servers")]
    public class TestCommand
    {
        private GameService gameService;

        public TestCommand(GameService service) 
        {
            this.gameService = service;
        
        
        }


        [Option("--verbose", "Enable verbose logging")]
        public bool Verbose { get; set; }

        [Command("start", "Starts the server")]
        public void Start(ICommandContext ctx, [Option("--force")] bool force)
        {
            if (Verbose)
                Console.WriteLine("Verbose mode enabled!");

            Console.WriteLine($"Starting server... Force={force}");
            Console.WriteLine($"Parsed command: {ctx.command.Name}");
            Console.WriteLine($"{gameService.IsInitialized}");
        }

        [Command("stop", "Stops the server")]
        public void Stop(ICommandContext ctx)
        {
            Console.WriteLine("Stopping server...");
        }
    }
}
