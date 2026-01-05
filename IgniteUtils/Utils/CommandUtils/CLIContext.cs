using InstanceUtils.Services.Commands;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using Torch2API.Models.Commands;

namespace InstanceUtils.Commands
{
    public class CLIContext : ICommandContext
    {

        public Command CLICommand { get; private set; }

        public CommandDescriptor Command { get; private set; }

        public ParseResult ParseResult { get; private set; }

        public string CommandName { get; private set; }

        public CommandTypeEnum CommandTypeContext => CommandTypeEnum.Console;

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
    }
}
