using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Utilities.CLI
{
    internal class CommandContext : ICommandContext
    {
        public Command command { get; set; }
        public ParseResult parseResult { get; set; }

        public CommandContext(Command command, ParseResult parseResult)
        {
            this.command = command;
            this.parseResult = parseResult;
        }
    }
}
