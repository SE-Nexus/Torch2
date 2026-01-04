using Google.Protobuf.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Reflection;
using Torch2API.Attributes;
using static VRage.Dedicated.Configurator.SelectInstanceForm;

namespace InstanceUtils.Commands
{
    public static class CommandLineBuilder
    {
        public static Command BuildFromType<T>(IServiceProvider services = null) where T : class
            => BuildFromType(typeof(T), services);


        public static Command BuildFromType(Type type, IServiceProvider services)
        {
            var groupAttr = type.GetCustomAttribute<CommandGroupAttribute>(true);
            if (groupAttr == null)
                throw new InvalidOperationException($"{type.Name} is missing [CommandGroup]");

            var groupCommand = new Command(groupAttr.Name, groupAttr.Description);

            // Create instance of T
            var instance = CreateInstance(type, services);


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



            // 2️. Subcommands (methods)
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                Command cmd;

                //Get sub command
                if (TryGetCommand(groupCommand, method, instance, out cmd))
                    continue;
            }

            // 3. TODO Implement recursive group commands



            return groupCommand;
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

        private static bool TryGetCommand(Command groupCommand, MethodInfo method, object instance, out Command command)
        {
            command = null;
            ICmdAttribute cmdAttr = method.GetCustomAttributes(true).OfType<ICmdAttribute>().FirstOrDefault();
            if (cmdAttr == null)
                return false;


            if (cmdAttr is CommandAttribute)
            {
                var subCommand = new Command(cmdAttr.Name, cmdAttr.Description);

                //Get options
                GenerateCmdOptions(method, subCommand);


                subCommand.SetAction((ParseResult result) =>
                {
                    //Get the method parameters attributes and parse the result for those values
                    //Then invoke the method with those parameters
                    CLIContext ctx = new CLIContext(subCommand, result);

                    List<object?> allMethodInputArgs = new List<object?>();
                    allMethodInputArgs.Add(ctx);

                    List<OptionAttribute> options = GetMethodOptions(method, subCommand);
                    foreach (var param in options)
                    {
                        allMethodInputArgs.Add(result.GetValue<object?>(param.Name));
                    }

                    method.Invoke(instance, allMethodInputArgs.ToArray());
                });

                groupCommand.Add(subCommand);

            }
            else
            {
                groupCommand.SetAction((ParseResult result) =>
                {
                    //Get the method parameters attributes and parse the result for those values
                    //Then invoke the method with those parameters

                    CLIContext ctx = new CLIContext(groupCommand, result);

                    List<object?> allMethodInputArgs = new List<object?>();
                    allMethodInputArgs.Add(ctx);


                    List<OptionAttribute> options = GetMethodOptions(method, groupCommand);
                    foreach (var param in options)
                    {
                        allMethodInputArgs.Add(result.GetValue<object?>(param.Name));
                    }

                    method.Invoke(instance, allMethodInputArgs.ToArray());
                });
            }



            return true;
        }

        private static List<OptionAttribute> GetMethodOptions(MethodInfo method, Command cmd)
        {
            List<OptionAttribute> allMethodOptions = new List<OptionAttribute>();
            foreach (var param in method.GetParameters())
            {
                var optAttr = param.GetCustomAttribute<OptionAttribute>(true);
                if (optAttr == null)
                    continue;

                allMethodOptions.Add(optAttr);
            }
            return allMethodOptions;
        }

        private static void GenerateCmdOptions(MethodInfo method, Command cmd)
        {
            foreach (var param in method.GetParameters())
            {
                var optAttr = param.GetCustomAttribute<OptionAttribute>(true);
                if (optAttr == null)
                    continue;

                var optionType = typeof(Option<>).MakeGenericType(param.ParameterType);

                //Set the type in the option
                optAttr.SetType(param.ParameterType);

                var option = (Option)Activator.CreateInstance(optionType, new[] { optAttr.Name });
                option.Description = optAttr.Description;
                cmd.Options.Add(option);
            }
        }







    }
}
