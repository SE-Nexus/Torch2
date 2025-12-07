using System;
using System.Collections.Generic;
using System.Text;

namespace IgniteUtils.Commands
{
    public interface ICommandContext
    {
        //Maybe replace with enum for discord, cli, etc
        bool IsConsoleCommand { get; }

        string CommandName { get; }




        void Respond(string response);



    }
}
