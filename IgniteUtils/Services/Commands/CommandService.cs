using InstanceUtils.Commands;
using InstanceUtils.Services.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Torch2API.Attributes;
using Torch2API.Models.Commands;

namespace InstanceUtils.Services
{
    /// <summary>
    /// Provides a background service for executing application commands asynchronously.
    /// </summary>
    /// <remarks>Inherit from this class to implement long-running command processing logic that runs in the
    /// background. The service is started automatically when the host starts and stopped when the host shuts down.
    /// Override the ExecuteAsync method to define the command execution behavior.</remarks>
    public class CommandService
    {

        private IServiceProvider _serviceProvider;
        public RootCommand CLIRoot { get; private set; }

        //TODO: Need a way to merge commands with the same group name
        public Dictionary<string, CommandGroupDescriptor> CommandGroups { get; private set; } = new Dictionary<string, CommandGroupDescriptor>(StringComparer.OrdinalIgnoreCase);

        public CommandService(IServiceProvider sProvider)
        {
            _serviceProvider = sProvider;

            Init();
        }

        public void Init()
        {
            CommandGroups.Clear();
            DiscoverCoommands();
            BuildCLICommands();
        }

        public void BuildCLICommands()
        {
            CLIRoot = new RootCommand("IgniteSE1 CLI");

            foreach (var group in CommandGroups.Values)
            {
                Command cmd = group.BuildCLI(CLIAction);

                //Lets not add anything if we dont have any
                if (cmd.Arguments.Count == 0 && cmd.Subcommands.Count == 0)
                    continue;

                CLIRoot.Add(cmd);
            }

        }

        /// <summary>
        /// Executes the command-line action based on the specified parse result and returns the exit code
        /// asynchronously.
        /// </summary>
        /// <param name="parseResult">The result of parsing command-line input, containing the command and arguments to execute.</param>
        /// <param name="cts">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the exit code for the
        /// command-line action.</returns>
        public Task<int> CLIAction(Command cli, CommandDescriptor cmd, ParseResult parseResult, CancellationToken cts)
        {
            //Prob need to pass in the cancellation token somewhere
            CLIContext ctx = new CLIContext(cli, cmd, parseResult);
            ctx.RunCommand(_serviceProvider);

            return Task.FromResult(0);
        }

        public void DiscoverCoommands()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && t.IsClass);

            foreach (var type in types)
            {
                var groupAttr = type.GetCustomAttribute<CommandGroupAttribute>(true);
                if (groupAttr == null)
                    continue;

                BuildCommandsFromType(type);
            }
        }

        public void BuildCommandsFromType(Type type)
        {
            if (type == null) return;

            var groupAttr = type.GetCustomAttribute<CommandGroupAttribute>(true);
            if (groupAttr == null)
                throw new InvalidOperationException($"{type.Name} is missing [CommandGroup]");


            CommandGroupDescriptor? current = null;
            var groupPath = groupAttr.Name.Split('/');

            for (int i = 0; i < groupPath.Length; i++)
            {
                var name = groupPath[i];
                bool isLeaf = i == groupPath.Length - 1;

                //This is the root group. It should be empty
                if (i == 0)
                {

                    // Root group. Add if does not exist
                    if (!CommandGroups.TryGetValue(name, out var root))
                    {
                        root = new CommandGroupDescriptor(name);
                        CommandGroups.Add(name, root);
                    }

                    current = root;
                }
                else
                {
                    var existing = current!.GetSubGroup(name);
                    if (existing == null)
                    {
                        existing = new CommandGroupDescriptor(name);
                        current.AddSubGroup(existing);
                    }

                    current = existing;
                }

                // Only apply attribute metadata to the leaf node
                if (isLeaf)
                {
                    current!.Description = groupAttr.Description;
                    current.CommandTypeOverride = groupAttr.CommandTypeOverride;
                }

            }

            /*
            //TODO: Implement group-level options if needed
            // 1️. Group-level options (properties)
            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var optAttr = prop.GetCustomAttribute<OptionAttribute>(true);
                if (optAttr == null)
                    continue;

                //We create an option of the appropriate type
                var optionType = typeof(Option<>).MakeGenericType(prop.PropertyType);
                var option = (Option)Activator.CreateInstance(optionType, new[] { optAttr.Name });
                option.Description = optAttr.Description;



                //Add any options to the group command
                groupCommand.Options.Add(option);
            }
             

            */



            //2. SubCommands (methods)
            //Not sure if i should do static methods
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                current.TryBuildCommand(method);
            }
        }

        /// <summary>
        /// The proto service will call this to execute a CLI command
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<string> InvokeCLICommand(string[] args)
        {
            CLIRoot.Description = "Remote IgniteSE1 Command Line Interface";
            var result = CLIRoot.Parse(args);


            StringWriter stringWriter = new StringWriter();
            InvocationConfiguration confg = new InvocationConfiguration();
            confg.Output = stringWriter;
            confg.Error = stringWriter; // optional, if you want stderr combined too

            await result.InvokeAsync(confg);
            return stringWriter.ToString();
        }



        public bool TryGetCommand(string commandpath, out CommandDescriptor cmd)
        {
            cmd = null!;

            if (string.IsNullOrWhiteSpace(commandpath))
                return false;

            var parts = commandpath
                .Split('.');

            if (parts.Length == 0)
                return false;

            // Start at root
            if (!CommandGroups.TryGetValue(parts[0], out var currentGroup))
                return false;

            // Traverse groups until last segment
            for (int i = 1; i < parts.Length - 1; i++)
            {
                if (!currentGroup.SubGroups.TryGetValue(parts[i], out var nextGroup))
                    return false;

                currentGroup = nextGroup;
            }

            // Final segment is the command
            // Avoid using C# 8.0 index-from-end operator for compatibility
            return currentGroup.Commands.TryGetValue(parts[parts.Length - 1], out cmd);
        }
    }
}
