using Grpc.Core;
using HarmonyLib;
using IgniteSE1.Services;
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

namespace IgniteSE1.Utilities
{

    public class CommandLineManager
    {
        private RootCommand root = new RootCommand("IgniteSE1 CLI");
        private ConsoleManager consoleManager;


        public CommandLineManager(ConsoleManager console)
        {
            //initialize command line arguments and options here
            consoleManager = console;
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

        public async Task ProcessCLICommand(string[] args)
        {

            Channel channel = new Channel($"localhost:{consoleManager.configs.Config.ProtoServerPort}", ChannelCredentials.Insecure);
            var client = new CommandLine.CommandLineClient(channel);

            var request = new CLIRequest();
            request.Command.AddRange(args);   // ✅ this is the key line

            var reply = await client.ProcessCLIAsync(request);
            AnsiConsole.WriteLine(reply.Result);
        }

        public Task<string> ProcessServerArgs(string[] args)
        {
            return InvokeCLICommand(args);
        }

        private async Task<string> InvokeCLICommand(string[] args)
        {
            root.Description = "Remote IgniteSE1 Command Line Interface";
            var result = root.Parse(args);

            StringWriter stringWriter = new StringWriter();
            InvocationConfiguration confg = new InvocationConfiguration();
            confg.Output = stringWriter;
            confg.Error = stringWriter; // optional, if you want stderr combined too

            await result.InvokeAsync(confg);
            return stringWriter.ToString();
        }
    }
}
