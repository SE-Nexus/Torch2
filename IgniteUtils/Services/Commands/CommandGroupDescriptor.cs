using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Torch2API.Attributes;
using Torch2API.Models.Commands;

namespace InstanceUtils.Services.Commands
{
    public class CommandGroupDescriptor
    {
        public string Name { get; }
        public string Description { get; }

        public CommandTypeEnum? CommandTypeOverride { get; private set; } = null;
        public bool HasCommandTypeOverride => CommandTypeOverride.HasValue;

        public CommandGroupDescriptor? Parent { get; private set; }

        public List<CommandDescriptor> Commands { get; } = new List<CommandDescriptor>();

        public List<CommandGroupDescriptor> SubGroups { get; } = new List<CommandGroupDescriptor>();



        public CommandGroupDescriptor(string name, string description, CommandTypeEnum? cmdType)
        {
            Name = name;
            Description = description;
            CommandTypeOverride = cmdType;
        }


        public void SetCommandTypeOverride(CommandTypeEnum commandType)
        {
            CommandTypeOverride = commandType;
        }

        public void AddCommand(CommandDescriptor command)
        {
            Commands.Add(command);
            command.ParentGroup = this;
        }

        public void AddSubGroup(CommandGroupDescriptor group)
        {
            SubGroups.Add(group);
            group.Parent = this;
        }

        public bool TryBuildCommand(MethodInfo method)
        {
            CommandDescriptor command = null;
            ICmdAttribute cmdAttr = method.GetCustomAttributes(true).OfType<ICmdAttribute>().FirstOrDefault();
            if (cmdAttr == null)
                return false;

            if (cmdAttr is CommandAttribute)
            {
                //Single Action Command
                try
                {
                    command = new CommandDescriptor(cmdAttr.Name, cmdAttr.Description, method, method.DeclaringType, cmdAttr.CommandType);

                    if (HasCommandTypeOverride)
                    {
                        command.SetCommandType(CommandTypeOverride.Value);
                    }

                    command.GetMethodOptions();

                    AddCommand(command);
                    return true;
                }
                catch (Exception ex)
                {
                    AnsiConsole.WriteLine($"Error building command {cmdAttr.Name}: {ex.Message}");
                    return false;
                }

            }
            else if (cmdAttr is GroupCommandActionAttribute)
            {
                //Group Action command

                return false;
            }

            return false;
        }



        #region CLI

        /// <summary>
        /// Builds and returns the root command-line interface (CLI) command for the application.
        /// </summary>
        /// <returns>A <see cref="Command"/> representing the root command of the CLI, including all configured subcommands and
        /// options.</returns>
        public Command BuildCLI(Func<Command, CommandDescriptor, ParseResult, CancellationToken, Task<int>> commandActionDelegate)
        {
            return BuildCLICommand(this, commandActionDelegate);
        }

        /// <summary>
        /// Builds a root command for a command-line interface (CLI) group, including all subcommands defined in the
        /// specified group descriptor.
        /// </summary>
        /// <param name="group">The descriptor that defines the CLI command group, including its name, description, and subcommands to be
        /// included.</param>
        /// <param name="commandActionDelegate">A delegate that represents the action to execute for each command. The delegate receives the parse result
        /// and a cancellation token, and returns a task that produces the command's exit code.</param>
        /// <returns>A Command object representing the root CLI command for the specified group, with all valid subcommands
        /// added.</returns>
        private static Command BuildCLICommand(CommandGroupDescriptor group, Func<Command, CommandDescriptor, ParseResult, CancellationToken, Task<int>> commandActionDelegate)
        {
            // Implementation for building CLI command


            // 1. Need to create group level options. IGNORE FOR NOW

            // 2. Create the sub commands


            var groupCommand = new Command(group.Name, group.Description);

            foreach (var commandDescriptor in group.Commands)
            {
                (bool success, Command cmd) = commandDescriptor.TryBuildCLICommand(commandActionDelegate);

                if (!success)
                    continue;

                groupCommand.Add(cmd);
            }

            return groupCommand;
        }

        #endregion

        public override string ToString()
        {
            return Name;
        }



    }
}
