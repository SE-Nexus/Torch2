using IgniteSE1.Utilities.Annotations;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Utilities.CLI
{
    public static class CommandLineBuilder
    {

        public static Command BuildFromType<T>() where T : class
        {
            var type = typeof(T);
            var groupAttr = type.GetCustomAttribute<CommandGroupAttribute>();
            if (groupAttr == null)
                throw new InvalidOperationException($"{type.Name} is missing [CommandGroup]");

            var groupCommand = new Command(groupAttr.Name, groupAttr.Description);

            // Create instance of T
            var instance = (T)Activator.CreateInstance(typeof(T), nonPublic: true);


            // 1️. Group-level options (properties)
            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var optAttr = prop.GetCustomAttribute<OptionAttribute>();
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
                var cmdAttr = method.GetCustomAttribute<CommandAttribute>();
                if (cmdAttr == null)
                    continue;

                var subCommand = new Command(cmdAttr.Name, cmdAttr.Description);

                List<OptionAttribute> allMethodOptions = new List<OptionAttribute>();
                foreach (var param in method.GetParameters())
                {
                    var optAttr = param.GetCustomAttribute<OptionAttribute>();
                    if (optAttr == null)
                        continue;

                    var optionType = typeof(Option<>).MakeGenericType(param.ParameterType);

                    //Set the type in the option
                    optAttr.SetType(param.ParameterType);

                    var option = (Option)Activator.CreateInstance(optionType, new[] { optAttr.Name });
                    option.Description = optAttr.Description;
                    subCommand.Options.Add(option);

                    allMethodOptions.Add(optAttr);
                }


                //correct action for System.CommandLine 2.x
                subCommand.SetAction((ParseResult result) =>
                {
                    //Get the method parameters attributes and parse the result for those values
                    //Then invoke the method with those parameters

                    CommandContext ctx = new CommandContext(subCommand, result);

                    List<object?> allMethodInputArgs = new List<object?>();
                    allMethodInputArgs.Add(ctx);

                    foreach (var param in allMethodOptions)
                    {
                        allMethodInputArgs.Add(result.GetValue<object?>(param.Name));
                    }

                    method.Invoke(instance, allMethodInputArgs.ToArray());
                });

                groupCommand.Add(subCommand);
            }






            return groupCommand;
        }


        


        


    }
}
