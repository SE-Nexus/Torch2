using Grpc.Core;
using InstanceUtils.Services;
using MyGrpcApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Services.ProtoServices
{
    public class CommandLineProtoService : CommandLine.CommandLineBase
    {
        private ConsoleManager _console;
        private CommandService _cli;

        public CommandLineProtoService(ConsoleManager console, CommandService cli)
        {
            _console = console;
            _cli = cli;
        }

        public override async Task<CLIReply> ProcessCLI(CLIRequest request, ServerCallContext context)
        {
            try
            {
                string result = await _cli.InvokeCLICommand(request.Command.ToArray());

                return new CLIReply
                {
                    Result = result
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[gRPC Handler Error] {ex}");
                throw;
            }
           
        }
      


    }
}
