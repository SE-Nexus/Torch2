using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace IgniteUtils.Commands
{
    public class CLIContext : ICommandContext
    {

        public Command Command { get; private set; }
        public ParseResult ParseResult { get; private set; }

        public bool IsConsoleCommand => true;

        public string CommandName { get; private set; }



        public CLIContext(Command command, ParseResult parseResult) 
        {
            Command = command;
            ParseResult = parseResult;
            CommandName = command.Name;
        }


        public void Respond(string response)
        {
            ParseResult.InvocationConfiguration.Output.Write(response);
        }
    }
}
