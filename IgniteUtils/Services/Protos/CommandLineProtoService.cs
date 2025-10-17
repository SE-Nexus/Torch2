using Grpc.Core;
using IgniteUtils.Services;
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
        private CommandLineManager _cli;

        public CommandLineProtoService(ConsoleManager console, CommandLineManager cli)
        {
            _console = console;
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
