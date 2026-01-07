using InstanceUtils.Services.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using Torch2API.Models.Commands;

namespace Torch2API.Models.Commands
{
    public interface ICommandContext
    {
        CommandTypeEnum CommandTypeContext { get; }   

        string CommandName { get; }

        CommandDescriptor Command { get; }




        void Respond(string response);

        void RespondLine(string response);

    }
}
