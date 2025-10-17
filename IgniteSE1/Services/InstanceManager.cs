using HarmonyLib;
using IgniteSE1.Configs;
using IgniteSE1.Utilities;
using IgniteUtils.Logging;
using IgniteUtils.Services;
using Microsoft.Extensions.Logging;
using NLog;
using Sandbox.Engine.Utils;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.FileSystem;
using VRage.Game;
using VRage.Game.ModAPI;
using static VRage.Dedicated.Configurator.SelectInstanceForm;

namespace IgniteSE1.Services
{
    [HarmonyPatch]
    public class InstanceManager : ServiceBase
    {
        private const string _instanceCfgFilename = "instancecfg.yaml";
        private const string _DedicatedCfgFilename = "SpaceEngineers-Dedicated.cfg";
        private const string _worldSavesFolder = "Saves";

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private ConfigService _configs;
        private string _instanceDirectory;

        private List<InstanceCfg> _instances = new List<InstanceCfg>();
        private InstanceCfg _selectedInstance = null;

        public InstanceManager(ConfigService configs)
        {
            _configs = configs;
            _instanceDirectory = _configs.Config.Directories.Instances;

            Directory.CreateDirectory(_instanceDirectory);
            LoadAllInstances();
        }

        public override Task<bool> Init()
        {
            //If user error lets fill out a default instance
            if (string.IsNullOrWhiteSpace(_configs.Config.TargetInstance))
                _configs.Config.TargetInstance = "MyNewIgniteInstance";

            //Load all instances from the instances directory
            if (_instances.Count == 0)
            {
                _logger.Warn("No instances found. Creating a default instance...");

                (bool result, string msg) = TryCreateNewInstance(_configs.Config.TargetInstance, out _selectedInstance);

                if (!result)
                {
                    _logger.Error($"Failed to create default instance: {msg}");
                    return Task.FromResult(false);
                }

            }
            else
            {
                _logger.Info($"Loaded {_instances.Count} instances.");

                //Check if the selected instance exists
                if (!TryGetInstanceByName(_configs.Config.TargetInstance, out _selectedInstance))
                {
                    _logger.Warn($"Failed to load target instance {_configs.Config.TargetInstance}. Does this exist?");
                    return Task.FromResult(false);
                }
            }


            return Task.FromResult(true);
        }


        private void LoadAllInstances()
        {
            //Clear existing instances
            _instances.Clear();

            //Get all directories in the instances folder
            Directory.GetDirectories(_instanceDirectory).ToList().ForEach(dir =>
            {
                string configFilePath = Path.Combine(dir, _instanceCfgFilename);
                if (File.Exists(configFilePath))
                {
                    try
                    {
                        InstanceCfg instanceCfg = InstanceCfg.LoadYaml(configFilePath);
                        instanceCfg.InstancePath = dir;
                        instanceCfg.InstanceName = Path.GetFileName(dir);
                        _instances.Add(instanceCfg); // Add the loaded instance configuration to the list
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to load instance configuration from {configFilePath}: {ex.Message}");
                    }
                }
            });
        }

        public (bool, string) TryCreateNewInstance(string InstanceName, out InstanceCfg cfg)
        {
            cfg = new InstanceCfg();

            //Null or empty check
            if (string.IsNullOrWhiteSpace(InstanceName))
                return (false, "Null Name");

            // Remove invalid filesystem characters
            string cleaned = string.Concat(InstanceName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));

            // Trim and normalize
            cleaned = cleaned.Trim();

            // Fallback if empty
            if (string.IsNullOrWhiteSpace(cleaned))
                return (false, "Invalid Instance Name");

            // Check if the instance name already exists
            (string fullPath, string NeName) = GetUniqueFolder(_instanceDirectory, cleaned);


            try
            {
                Directory.CreateDirectory(fullPath);
                Directory.CreateDirectory(Path.Combine(fullPath, _worldSavesFolder)); // Create Saves directory

                cfg.filePath = Path.Combine(fullPath, _instanceCfgFilename); // Set the file path for the instance configuration
                cfg.InstanceName = NeName;
                cfg.InstancePath = Path.Combine(_instanceDirectory, InstanceName); // Set the instance path in the config
                cfg.Save(); // Save the configuration to the file

                _instances.Add(cfg); // Add the new instance configuration to the list
                return (true, "Instance Created Successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.ToString());
            }
        }

        public static (string, string) GetUniqueFolder(string basePath, string folderName)
        {
            string fullPath = Path.Combine(basePath, folderName);
            string newName = folderName;
            int count = 1;

            while (Directory.Exists(fullPath))
            {
                newName = $"{folderName}-{count}";
                fullPath = Path.Combine(basePath, newName);
                count++;
            }

            return (fullPath, newName);
        }


        public bool TryGetSelectedInstance(out InstanceCfg targetInstance)
        {
            targetInstance = null; // Initialize the target instance to null

            // This method should return the currently selected instance
            // For now, we will just log the selected instance name

            //Need some null checks here
            string instanceName = _configs.Config.TargetInstance;

            if (string.IsNullOrEmpty(instanceName))
            {
                _logger.InfoColor("No instance is currently selected.", Color.Yellow);
                return false;
            }

            if (!TryGetInstanceByName(instanceName, out targetInstance))
                _logger.Warn($"Target Instance {instanceName} does not exist. Please select another or create a new one.");

            return targetInstance != null;
        }

        public bool TryGetInstanceByName(string instanceName, out InstanceCfg cfg)
        {
            // This method should return the instance configuration by name
            cfg = _instances.FirstOrDefault(instance => instance.InstanceName.Equals(instanceName, StringComparison.OrdinalIgnoreCase));
            return cfg != null;
        }




        public InstanceCfg GetCurrentInstance()
        {
            return _selectedInstance;
        }

        public IMyConfigDedicated GetServerConfigs()
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _selectedInstance.InstancePath, _DedicatedCfgFilename);
            var gameconfig = new MyConfigDedicated<MyObjectBuilder_SessionSettings>(configPath);

            /// Load or create the config file
            if (File.Exists(configPath))
            {
                gameconfig.Load();
            }
            else
            {
                gameconfig.Save();
            }


            gameconfig.WorldName = _selectedInstance.TargetWorld;
            gameconfig.LoadWorld = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _selectedInstance.InstancePath, _worldSavesFolder, _selectedInstance.TargetWorld);


            if (_selectedInstance == null)
                return null;

            return gameconfig;
        }


        [HarmonyPatch(typeof(MyFileSystem), "Init")]
        private static void InitFileSystem_Prefix(string contentPath, string userData, string modDirName = "Mods", string shadersBasePath = null, string modsCachePath = null)
        {
            //Console.WriteLine($"[Harmony] MyFileSystem.Init called with contentPath: {contentPath}, userData: {userData}");
        }


        public List<InstanceCfg> GetAllInstances()
        {
            return _instances;
        }

    }
}
