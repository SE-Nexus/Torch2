using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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

        public void Respond(string response) { }
        public void RespondLine(string response) { }

        public void RunCommand(IServiceProvider serviceProvider)
        {
            var allMethodInputArgs = new List<object?>();

            using (var scope = serviceProvider.CreateScope())
            {
                var accessor = scope.ServiceProvider.GetRequiredService<CommandContextAccessor>();
                accessor.context = this;

                var declaringInstance = ServiceExtensions.CreateInstance(Command.DeclaringType, scope.ServiceProvider);

                

                if (socketmsg.Args.ValueKind is not JsonValueKind.Object)
                    throw new InvalidOperationException($"WS command '{socketmsg.Command}' missing args object.");

                for (int i = 0; i < Command.Options.Length; i++)
                {
                    var option = Command.Options[i];

                    // Use "--worldname" => "worldname" key
                    var key = NormalizeOptionKey(option.Name);

                    if (!socketmsg.Args.TryGetProperty(key, out var argElement))
                    {
                        if (option.HasDefaultValue)
                        {
                            allMethodInputArgs.Add(
                                option.DefaultValue == DBNull.Value
                                    ? GetDefault(option.OptionType)
                                    : option.DefaultValue);
                            continue;
                        }

                        throw new InvalidOperationException(
                            $"Missing required argument '{option.Name}' (key '{key}') for command '{Command.Name} @ {Command.DeclaringType.FullName}'.");
                    }

                    var value = argElement.Deserialize(option.OptionType, TorchConstants.JsonOptions);
                    allMethodInputArgs.Add(value);
                }

                Command.Method.Invoke(declaringInstance, allMethodInputArgs.ToArray());
            }
        }

        private static string NormalizeOptionKey(string optionName)
        {
            var key = (optionName ?? string.Empty).Trim();
            while (key.StartsWith("-", StringComparison.Ordinal))
                key = key.Substring(1);

            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Option name produced an empty argument key.");

            return key;
        }

        private static object? GetDefault(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }
    }
}
