using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Utilities.CLI
{
    public interface ICommandContext
    {
        Command command { get; }
        ParseResult parseResult { get; }

        
        


    }
}
