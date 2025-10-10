using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Utilities
{

    public class CommandLineManager
    {
        RootCommand root = new RootCommand("IgniteSE1 CLI");


        public CommandLineManager()
        {
            //initialize command line arguments and options here

            
        }

        public async Task ProcessCLICommand(string[] args)
        {
            root.Description = "Remote IgniteSE1 Command Line Interface";

            var result = root.Parse(args);

            await result.InvokeAsync();

        }

        public void ProcessServerArgs(string[] args)
        {

        }

       





    }
}
