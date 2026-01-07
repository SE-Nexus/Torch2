using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Torch2API.Constants;
using Torch2API.DTOs.WebSockets;
using Torch2API.Models.Commands;

namespace InstanceUtils.Services.Commands.Contexts
{
    public class WebPanelContext : ICommandContext
    {
        public CommandTypeEnum CommandTypeContext => CommandTypeEnum.WebPanel;

        public string CommandName => Command.Name;

        public CommandDescriptor Command { get; private set; }

        public SocketMsgEnvelope socketmsg { get; private set; }


        public WebPanelContext(CommandDescriptor Command, SocketMsgEnvelope msg)
        {
            this.Command = Command;
            this.socketmsg = msg;
        }

        public void Respond(string response)
        {
            //throw new NotImplementedException();
        }

        public void RespondLine(string response)
        {
            //throw new NotImplementedException();
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
                for (int i = 0; i < Command.Options.Length; i++)
                {
                    var option = Command.Options[i];

                    // If payload has a value at this index, deserialize it
                    if (i < socketmsg.Payload.Length)
                    {
                        var element = socketmsg.Payload[i];

                        var value = element.Deserialize(option.OptionType, TorchConstants.JsonOptions);
                        allMethodInputArgs.Add(value);
                    }
                    else
                    {
                        if (option.HasDefaultValue)
                        {
                            allMethodInputArgs.Add(
                                option.DefaultValue == DBNull.Value
                                    ? GetDefault(option.OptionType)
                                    : option.DefaultValue
    );
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Missing required argument '{option.Name}' " +
                                $"(position {i}) for command '{Command.Name} @ {Command.DeclaringType.FullName}'."
                            );
                        }
                    }
                }

                //Invoke the method
                Command.Method.Invoke(declaringInstance, allMethodInputArgs.ToArray());
            }
        }

        private static object? GetDefault(Type type)
        {
            // value types → default(T)
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            // reference types → null
            return null;
        }
    }
}
