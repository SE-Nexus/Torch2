using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.ComponentModel.Design;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Torch2API.Attributes;
using Torch2API.Models.Commands;

namespace InstanceUtils.Services.Commands
{

    /// <summary>
    /// Represents the metadata and configuration for a command, including its name, description, declaring type,
    /// method, and available options.
    /// </summary>
    /// <remarks>Use this class to describe a command that can be invoked, along with its associated options
    /// and method information. This type is typically used in command-line frameworks or systems that support dynamic
    /// command discovery and execution.</remarks>
    public sealed class CommandDescriptor
    {
        public string Name { get; }

        public string Description { get; }

        public CommandGroupDescriptor ParentGroup { get; set; }

        public Type DeclaringType { get; }

        public MethodInfo Method { get; }

        public CommandTypeEnum? CommandType { get; private set; }

        public CommandOptionDescriptor[] Options { get; set; }



        public CommandDescriptor(string name, string description, MethodInfo method, Type DeclaringType, CommandTypeEnum? commandType)
        {
            Name = name;
            Description = description;
            Method = method;
            this.DeclaringType = DeclaringType;
            CommandType = commandType;
        }

        public void GetMethodOptions()
        {
            if (Method == null)
                throw new InvalidOperationException("Method information is not set for this command descriptor.");


            var optionList = new List<CommandOptionDescriptor>();
            var parameters = Method.GetParameters();
            foreach (var param in parameters)
            {
                var optAttr = param.GetCustomAttribute<OptionAttribute>(true);
                if (optAttr == null)
                    continue;

                var optionType = param.ParameterType;
                var optionDescriptor = new CommandOptionDescriptor(optAttr.Name, param.Name, optAttr.Description, optionType, param.DefaultValue);
                optionList.Add(optionDescriptor);
            }

            Options = optionList.ToArray();
        }

        public (bool, Command) TryBuildCLICommand(Func<Command, CommandDescriptor, ParseResult, CancellationToken, Task<int>> CommandActionDelegeate)
        {

            CommandTypeEnum valueEnum;
            if (!CommandType.HasValue)
                return (false, null);
            else
                valueEnum = CommandType.Value;

            //This isnt a valid CLI Command
            if (!valueEnum.HasFlag(CommandTypeEnum.Console))
                return (false, null);

            Command cmd = new Command(Name, Description);

            // Set the action for the command. This is being passed from the command service to link back to the command execution logic.
            cmd.SetAction((context, cts) =>
            {
                return CommandActionDelegeate(cmd, this, context, cts);
            });


            try
            {

                foreach (var cmdoption in Options)
                {
                    var optionType = typeof(Option<>).MakeGenericType(cmdoption.OptionType);
                    var option = (Option)Activator.CreateInstance(optionType, new[] { cmdoption.Name });
                    option.Description = cmdoption.Description;

                    cmd.Options.Add(option);
                }

                return (true, cmd);

            }catch (Exception ex)
            {
                AnsiConsole.WriteLine($"Error building CLI command options for command '{Name}': {ex.Message}");
                return (false, null);
            }
        }

        public void SetCommandType(CommandTypeEnum commandType)
        {
            CommandType = commandType;
        }
    }
}
