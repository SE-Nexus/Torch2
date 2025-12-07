using Grpc.Core;
using HarmonyLib;
using MyGrpcApp;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sandbox.Game.World.MyWorldGenerator;
using Microsoft.Extensions.DependencyInjection;
using IgniteUtils.Services;
using IgniteUtils.Utils.CommandUtils;

namespace IgniteUtils.Services
{

    public class CommandLineManager : ServiceBase
    {
        public RootCommand RootCommand { get; private set; } = new RootCommand("IgniteSE1 CLI");
        int ProtoServicePort;


        public CommandLineManager(int ProtoServicePort)
        {
            //initialize command line arguments and options here
            this.ProtoServicePort = ProtoServicePort;
        }



        public async Task SetupCommandLineManager(bool isServer, string[] args)
        {
            if (isServer)
            {

            }
            else
            {
               await ProcessCLICommand(args);
            }
        }

        private void SetupCommands()
        {

        }

        public async Task ProcessCLICommand(string[] args)
        {
            if (args.Length == 0)
                return;


            //Check if interactive mode
            if (args[0].Equals("--interactive", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.Write(new Panel("[bold green]Interactive mode enabled![/] [grey]Type [yellow]exit[/] to quit.[/]").Border(BoxBorder.Rounded).Header("[white on green] CLI Mode [/]"));

                while (true)
                {
                    var input = AnsiConsole.Prompt(
                            new TextPrompt<string>("[grey]>[/]")
                                .PromptStyle("deepskyblue1")
                        );

                    if(string.IsNullOrEmpty(input))
                        continue;

                    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                        break;

                    string[] inputArgs = input.Split(new char[] { ' '}, StringSplitOptions.RemoveEmptyEntries);
                    await SendCLIRequest(inputArgs);
                }

                //Lets return. As the command would attempt to continue with the --interactive stuff
                return;
            }else if (args[0].Equals("--snake", StringComparison.OrdinalIgnoreCase))
            {
                SnakeGame game = new SnakeGame();
                game.Run();
                return;
            }


                //Send singular command;
                await SendCLIRequest(args);
        }

        private async Task SendCLIRequest(string[] args)
        {
            Channel channel = new Channel($"localhost:{ProtoServicePort}", ChannelCredentials.Insecure);
            var client = new CommandLine.CommandLineClient(channel);

            var request = new CLIRequest();
            request.Command.AddRange(args);   // ✅ this is the key line

            var reply = await client.ProcessCLIAsync(request);
            AnsiConsole.WriteLine(reply.Result);
        }


        

        public async Task<string> InvokeCLICommand(string[] args)
        {
            RootCommand.Description = "Remote IgniteSE1 Command Line Interface";
            var result = RootCommand.Parse(args);


            StringWriter stringWriter = new StringWriter();
            InvocationConfiguration confg = new InvocationConfiguration();
            confg.Output = stringWriter;
            confg.Error = stringWriter; // optional, if you want stderr combined too

            await result.InvokeAsync(confg);
            return stringWriter.ToString();
        }



    }
}
