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

                // Get method parameters in order
                var methodParams = Command.Method.GetParameters();

                // Process parameters in the order they appear in the method signature
                foreach (var param in methodParams)
                {
                    // Try to find a matching Input first
                    var inputDescriptor = System.Array.Find(Command.Inputs, i => i.ArgName == param.Name);
                    if (inputDescriptor != null)
                    {
                        var key = NormalizeOptionKey(inputDescriptor.Name);
                        if (!socketmsg.Args.TryGetProperty(key, out var argElement))
                        {
                            if (inputDescriptor.HasDefaultValue)
                            {
                                allMethodInputArgs.Add(
                                    inputDescriptor.DefaultValue == DBNull.Value
                                        ? GetDefault(inputDescriptor.InputType)
                                        : inputDescriptor.DefaultValue);
                                continue;
                            }

                            throw new InvalidOperationException(
                                $"Missing required input '{inputDescriptor.Name}' (key '{key}') for command '{Command.Name} @ {Command.DeclaringType.FullName}'.");
                        }

                        var value = argElement.Deserialize(inputDescriptor.InputType, TorchConstants.JsonOptions);
                        allMethodInputArgs.Add(value);
                        continue;
                    }

                    // Try to find a matching Option
                    var optionDescriptor = System.Array.Find(Command.Options, o => o.ArgName == param.Name);
                    if (optionDescriptor != null)
                    {
                        var key = NormalizeOptionKey(optionDescriptor.Name);
                        if (!socketmsg.Args.TryGetProperty(key, out var argElement))
                        {
                            if (optionDescriptor.HasDefaultValue)
                            {
                                allMethodInputArgs.Add(
                                    optionDescriptor.DefaultValue == DBNull.Value
                                        ? GetDefault(optionDescriptor.OptionType)
                                        : optionDescriptor.DefaultValue);
                                continue;
                            }

                            throw new InvalidOperationException(
                                $"Missing required option '{optionDescriptor.Name}' (key '{key}') for command '{Command.Name} @ {Command.DeclaringType.FullName}'.");
                        }

                        var value = argElement.Deserialize(optionDescriptor.OptionType, TorchConstants.JsonOptions);
                        allMethodInputArgs.Add(value);
                        continue;
                    }

                    // Parameter not found in Inputs or Options - add null
                    allMethodInputArgs.Add(null);
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
