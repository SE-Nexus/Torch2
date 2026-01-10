using HarmonyLib;
using IgniteSE1.Configs;
using InstanceUtils.Logging;
using InstanceUtils.Services;
using Microsoft.Extensions.Logging;
using NLog;
using Sandbox.Engine.Networking;
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
using Torch2API.Models.Configs;
using VRage.FileSystem;
using VRage.Game;
using VRage.Game.ModAPI;

namespace IgniteSE1.Services
{
    [HarmonyPatch]
    public class ProfileManager : ServiceBase
    {
        private const string _instanceCfgFilename = "instancecfg.yaml";
        private const string _DedicatedCfgFilename = "SpaceEngineers-Dedicated.cfg";

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private ConfigService _configs;
        private string _ProfileDirectory;

        private List<ProfileCfg> _instances = new List<ProfileCfg>();
        private List<WorldInfo> _worlds = new List<WorldInfo>();
        private ProfileCfg _selectedInstance = null;



        public ProfileManager(ConfigService configs)
        {
           

            _configs = configs;
            _ProfileDirectory = _configs.Config.Directories.ProfileDir;

            Directory.CreateDirectory(_ProfileDirectory);
            LoadAllProfiles();
            LoadAllWorlds();
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

                (bool result, string msg) = TryCreateNewProfile(_configs.Config.TargetInstance, out _selectedInstance);

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
                if (!TryGetProfileByName(_configs.Config.TargetInstance, out _selectedInstance))
                {
                    _logger.Warn($"Failed to load target instance {_configs.Config.TargetInstance}. Does this exist?");
                    return Task.FromResult(false);
                }
            }


            return Task.FromResult(true);
        }


        private void LoadAllProfiles()
        {
            //Clear existing instances
            _instances.Clear();

            //Get all directories in the instances folder
            Directory.GetDirectories(_ProfileDirectory).ToList().ForEach(dir =>
            {
                string configFilePath = Path.Combine(dir, _instanceCfgFilename);
                if (File.Exists(configFilePath))
                {
                    try
                    {
                        ProfileCfg instanceCfg = ProfileCfg.LoadYaml(configFilePath);
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

        public void LoadAllWorlds()
        {
            _worlds.Clear();
            foreach (var worldPath in Directory.GetDirectories(_configs.Config.Directories.WorldsDir))
            {
                var di = new DirectoryInfo(worldPath);

                WorldInfo worldInfo = new WorldInfo
                {
                    Name = di.Name,
                    CreatedUtc = di.CreationTime,
                    LastUpdatedUtc = di.LastWriteTimeUtc
                };

                _worlds.Add(worldInfo);
            }
        }

        public (bool, string) TryCreateNewProfile(string InstanceName, out ProfileCfg cfg)
        {
            cfg = new ProfileCfg();

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
            (string fullPath, string NeName) = GetUniqueFolder(_ProfileDirectory, cleaned);


            try
            {
                Directory.CreateDirectory(fullPath);

                cfg.filePath = Path.Combine(fullPath, _instanceCfgFilename); // Set the file path for the instance configuration
                cfg.InstanceName = NeName;
                cfg.InstancePath = Path.Combine(_ProfileDirectory, InstanceName); // Set the instance path in the config
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



        public bool TryGetSelectedProfile(out ProfileCfg targetInstance)
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

            if (!TryGetProfileByName(instanceName, out targetInstance))
                _logger.Warn($"Target Instance {instanceName} does not exist. Please select another or create a new one.");

            return targetInstance != null;
        }

        public bool TryGetProfileByName(string instanceName, out ProfileCfg cfg)
        {
            // This method should return the instance configuration by name
            cfg = _instances.FirstOrDefault(instance => instance.InstanceName.Equals(instanceName, StringComparison.OrdinalIgnoreCase));
            return cfg != null;
        }


        public bool TryCreateWorld(string worldname, string templatepath, out string reason)
        {
            reason = "";

            try
            {
                string dir = Path.Combine(_configs.Config.Directories.WorldsDir, worldname.Trim());
                if (Directory.Exists(dir))
                {
                    reason = $"World {worldname} already Exists. Please try a different name";
                    return false;
                }

                Directory.CreateDirectory(dir);

                var checkpoint = MyLocalCache.LoadCheckpoint(templatepath, out _);
                if (checkpoint == null)
                {
                    reason = $"Failed to load template checkpoint at {templatepath}";
                    return false;
                }


                //Copy everything from template over to the new world folder
                foreach (var file in Directory.EnumerateFiles(templatepath, "*", SearchOption.AllDirectories))
                {
                    // Trash code to work around inconsistent path formats.
                    var fileRelPath = file.Replace($"{templatepath.TrimEnd('\\')}\\", "");
                    var destPath = Path.Combine(dir, fileRelPath);
                    File.Copy(file, destPath);
                }

                //Default to public online
                checkpoint.OnlineMode = MyOnlineModeEnum.PUBLIC;
                checkpoint.SessionName = worldname;

                if (!MyLocalCache.SaveCheckpoint(checkpoint, dir))
                {
                    reason = $"Failed to save new world checkpoint";
                    return false;
                }

                return true;
            }catch(Exception ex)
            {
                _logger.Fatal(ex);
                return false;
            }
        }


        public ProfileCfg GetCurrentProfile()
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
            gameconfig.LoadWorld = Path.Combine(Path.GetFullPath(_configs.Config.Directories.WorldsDir), _selectedInstance.TargetWorld);


            if (_selectedInstance == null)
                return null;

            return gameconfig;
        }


        [HarmonyPatch(typeof(MyFileSystem), "Init")]
        private static void InitFileSystem_Prefix(string contentPath, string userData, string modDirName = "Mods", string shadersBasePath = null, string modsCachePath = null)
        {
            //Console.WriteLine($"[Harmony] MyFileSystem.Init called with contentPath: {contentPath}, userData: {userData}");
        }


        public List<ProfileCfg> GetAllInstances()
        {
            return _instances;
        }

        public List<WorldInfo> GetAllWorlds()
        {
            return _worlds; 
        }

    }
}
