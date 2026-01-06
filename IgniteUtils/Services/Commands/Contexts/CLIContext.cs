using InstanceUtils.Services.Commands;
using InstanceUtils.Services.Commands.Contexts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using Torch2API.Models.Commands;
using static SpaceEngineers.Game.VoiceChat.OpusDevice;

namespace InstanceUtils.Services.Commands
{
    public class CLIContext : ICommandContext
    {
        public CommandTypeEnum CommandTypeContext => CommandTypeEnum.Console;


        public Command CLICommand { get; private set; }

        public CommandDescriptor Command { get; private set; }

        public ParseResult ParseResult { get; private set; }

        public string CommandName { get; private set; }



        public CLIContext(Command command, CommandDescriptor cmdDescriptor, ParseResult parseResult)
        {
            CLICommand = command;
            Command = cmdDescriptor;
            ParseResult = parseResult;
            CommandName = command.Name;
        }


        public void Respond(string response)
        {
            ParseResult.InvocationConfiguration.Output.Write(response);
        }

        public void RunCommand(IServiceProvider serviceProvider)
        {
            List<object?> allMethodInputArgs = new List<object?>();

            //Create scope for DI
            using (var scope = serviceProvider.CreateScope())
            {
                //Set the context accessor
                var accessor = scope.ServiceProvider.GetRequiredService<CommandContextAccessor>();
                accessor.context = this;

                //Get instance of the declaring type
                var declaringInstance = ServiceExtensions.CreateInstance(Command.DeclaringType, scope.ServiceProvider);

                //Build method input args
                var methodParams = Command.Method.GetParameters();
                foreach (var arg in methodParams)
                {
                    var option = Command.Options.FirstOrDefault(o => o.ArgName == arg.Name);
                    if (option != null)
                    {
                        var value = ParseResult.GetValue<object?>(option.Name);
                        allMethodInputArgs.Add(value);
                    }
                    else
                    {
                        allMethodInputArgs.Add(null);
                    }

                }

                //Invoke the method
                Command.Method.Invoke(declaringInstance, allMethodInputArgs.ToArray());
            }
        }
    }
}
