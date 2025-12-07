using IgniteUtils.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace IgniteUtils.Utils.Configs
{
    public static class EnviromentVarLoader
    {
        public static void Load(object target)
        {
            if (target == null)
                return;

            Type type = target.GetType();
            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                // 1️⃣ Check if property has the EnvVarAttribute
                var attr = prop.GetCustomAttribute<EnvVarAttribute>();
                if (attr != null)
                {
                    string? value = Environment.GetEnvironmentVariable(attr.Name, attr.Target);
                    if (value != null && prop.CanWrite)
                    {
                        try
                        {
                            object converted = Convert.ChangeType(value, prop.PropertyType);
                            prop.SetValue(target, converted);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Failed to set {prop.Name} from {attr.Name}: {ex.Message}");
                        }
                    }
                    continue;
                }

                // 2️⃣ If property is a class (not primitive/string), recurse
                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    object? subObj = prop.GetValue(target);

                    // Auto-instantiate null nested objects
                    if (subObj == null && prop.CanWrite)
                    {
                        subObj = Activator.CreateInstance(prop.PropertyType);
                        prop.SetValue(target, subObj);
                    }

                    if (subObj != null)
                    {
                        Load(subObj);
                    }
                }
            }
        }
    }
}
