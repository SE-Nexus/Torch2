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

        public Dictionary<string, CommandGroupDescriptor> CommandGroups { get; private set; } = new Dictionary<string, CommandGroupDescriptor>();

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

            //Check if we already have this command group
            CommandGroupDescriptor cmdGroup;
            if (!CommandGroups.TryGetValue(groupAttr.Name, out cmdGroup))
            {
                cmdGroup = new CommandGroupDescriptor(groupAttr.Name, groupAttr.Description, groupAttr.CommandTypeOverride);
                CommandGroups.Add(groupAttr.Name, cmdGroup);
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
                cmdGroup.TryBuildCommand(method);
            }
        }

        

        public bool TryAddCommand(string command)
        {
            return false;
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
            CLIContext ctx = new CLIContext(cli, cmd, parseResult);

            List<object?> allMethodInputArgs = new List<object?>();

            using (var scope = _serviceProvider.CreateScope())
            {
                //Get instance of the declaring type
                var declaringInstance = CreateInstance(cmd.DeclaringType, scope.ServiceProvider);

                //Build method input args
                var methodParams = cmd.Method.GetParameters();
                foreach (var arg in methodParams)
                {
                    if (arg.ParameterType == typeof(ICommandContext))
                    {
                        allMethodInputArgs.Add(ctx);
                    }
                    else
                    {
                        var option = cmd.Options.FirstOrDefault(o => o.ArgName == arg.Name);
                        if (option != null)
                        {
                            var value = parseResult.GetValue<object?>(option.Name);
                            allMethodInputArgs.Add(value);
                        }
                        else
                        {
                            allMethodInputArgs.Add(null);
                        }
                    }
                }

                //Invoke the method
                cmd.Method.Invoke(declaringInstance, allMethodInputArgs.ToArray());
            }

            return Task.FromResult(0);
        }


        private static object CreateInstance(Type type, IServiceProvider? services)
        {
            try
            {
                //No DI container? Fallback to plain reflection
                if (services == null)
                    return Activator.CreateInstance(type, nonPublic: true)!;


                //just use the main provider. Maybe add scoped stuff in future
                return ActivatorUtilities.CreateInstance(services, type);
            }
            catch (Exception ex)
            {
                // 💥 Fallback to reflection if DI creation fails
                AnsiConsole.MarkupLineInterpolated(
                $"[bold red][[DI]][/] Failed to create instance of [yellow]{type.Name}[/]: {ex.Message}");

                if (services == null)
                    return null;

                try
                {
                    return Activator.CreateInstance(type, nonPublic: true)!;
                }
                catch (Exception innerEx)
                {
                    throw new InvalidOperationException(
                        $"Unable to create instance of type {type.FullName}. " +
                        $"DI failed and fallback instantiation also failed.",
                        innerEx);
                }
            }
        }




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
    }
}
