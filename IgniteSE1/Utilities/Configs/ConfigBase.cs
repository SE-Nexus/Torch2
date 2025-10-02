using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace IgniteSE1.Utilities
{
    public abstract class ConfigBase<T> where T : ConfigBase<T>, new()
    {
        [YamlIgnore]
        public string filePath;

        public void Save()
        {
            var serializer = new SerializerBuilder()
                 .WithNamingConvention(CamelCaseNamingConvention.Instance)
                 .Build();

            var yaml = serializer.Serialize(this);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)); // Ensure directory exists
            File.WriteAllText(filePath, yaml);
        }

        public static T LoadYaml(string filePath)
        {
            if (!File.Exists(filePath))
            {
                // Optionally create a default file
                var defaultInstance = new T();
                defaultInstance.filePath = filePath; // Set the file path for saving later
                defaultInstance.Save();
                return defaultInstance;
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var yaml = File.ReadAllText(filePath);
            return deserializer.Deserialize<T>(yaml);
        }

    }
}
