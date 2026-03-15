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
using static VRage.Dedicated.Configurator.SelectInstanceForm;

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

        //Changed Events
        public event Action<List<WorldInfo>>? WorldsChanged;
        public event Action<List<ProfileCfg>>? ProfilesChanged;



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


                ProfilesChanged?.Invoke(_instances); // Invoke the ProfilesChanged event to notify subscribers of the change
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

        /// <summary>
        /// Loads the profile named <paramref name="instanceName"/> and persists it as the configured TargetInstance.
        /// Returns false with a reason on failure.
        /// </summary>
        public bool TryLoadProfile(string instanceName, out string reason)
        {
            reason = string.Empty;

            if (string.IsNullOrWhiteSpace(instanceName))
            {
                reason = "Instance name is required.";
                return false;
            }

            if (!TryGetProfileByName(instanceName, out var profile))
            {
                reason = $"Profile '{instanceName}' not found.";
                return false;
            }


            try
            {
                _selectedInstance = profile;
                // Persist as the configured target so other systems pick it up
                _configs.Config.TargetInstance = profile.InstanceName;
                try
                {
                    _configs.Config.Save();
                }
                catch (Exception ex)
                {
                    // Non-fatal: log and continue (profile loaded in memory)
                    _logger.Warn(ex, $"Failed to persist TargetInstance setting to config: {ex.Message}");
                }

                ProfilesChanged?.Invoke(_instances);
                return true;
            }
            catch (Exception ex)
            {
                reason = ex.Message;
                _logger.Error(ex, $"Failed to load profile '{instanceName}'");
                return false;
            }
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

                var di = Directory.CreateDirectory(dir);

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

                //Add the updated world to the active worlds
                WorldInfo worldInfo = new WorldInfo
                {
                    Name = di.Name,
                    CreatedUtc = di.CreationTime,
                    LastUpdatedUtc = di.LastWriteTimeUtc
                };
                _worlds.Add(worldInfo);


                WorldsChanged.Invoke(_worlds); // Invoke the WorldsChanged event to notify subscribers of the change

                return true;
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex);
                return false;
            }
        }


        public bool TryDeleteWorld(string worldname, out string reason)
        {
            reason = "";

            try
            {
                if (string.IsNullOrWhiteSpace(worldname))
                {
                    reason = "World name is required.";
                    return false;
                }

                // Find the world in the in-memory list
                var world = _worlds.FirstOrDefault(w => string.Equals(w.Name, worldname, StringComparison.OrdinalIgnoreCase));
                if (world == null)
                {
                    reason = $"World '{worldname}' does not exist.";
                    return false;
                }

                // Build the world directory path
                string dir = Path.Combine(_configs.Config.Directories.WorldsDir, worldname);

                if (!Directory.Exists(dir))
                {
                    reason = $"World directory '{dir}' does not exist.";
                    return false;
                }

                // Delete the directory and all contents
                Directory.Delete(dir, true);

                // Remove from in-memory list
                _worlds.Remove(world);

                // Notify subscribers
                WorldsChanged?.Invoke(_worlds);

                return true;
            }
            catch (Exception ex)
            {
                reason = $"Failed to delete world '{worldname}': {ex.Message}";
                _logger.Error(ex, reason);
                return false;
            }
        }


        public ProfileCfg GetCurrentProfile()
        {
            return _selectedInstance;
        }

        public IMyConfigDedicated GetServerConfigs(ProfileCfg? targetInstance = null)
        {
            if (targetInstance == null)
            {
                targetInstance = GetCurrentProfile();
            }
                

            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, targetInstance.InstancePath, _DedicatedCfgFilename);
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

            gameconfig.WorldName = targetInstance.TargetWorld;
            gameconfig.LoadWorld = Path.Combine(Path.GetFullPath(_configs.Config.Directories.WorldsDir), targetInstance.TargetWorld);


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

        /// <summary>
        /// Attempts to apply an update to a profile with the specified instance name using the provided action.
        /// </summary>
        /// <remarks>If the profile's instance name is changed, this method attempts to update the
        /// corresponding directory name. If a profile with the new name already exists, the update is not applied and
        /// this method returns false. Any errors encountered during the update are captured in the reason
        /// parameter.</remarks>
        /// <param name="instanceName">The name of the profile instance to update. Cannot be null, empty, or whitespace.</param>
        /// <param name="apply">An action that applies modifications to the profile. The profile object is passed to this action for
        /// mutation.</param>
        /// <param name="reason">When this method returns, contains the reason for a failure if the update could not be applied; otherwise,
        /// an empty string.</param>
        /// <returns>true if the profile update succeeded; otherwise, false.</returns>
        public bool TryApplyProfileUpdate(string instanceName, Action<ProfileCfg> apply, out string reason)
        {
            reason = string.Empty;
            if (string.IsNullOrWhiteSpace(instanceName))
            {
                reason = "Instance name is required.";
                return false;
            }

            if (!TryGetProfileByName(instanceName, out var profile))
            {
                reason = $"Profile '{instanceName}' not found.";
                return false;
            }

            string originalName = profile.InstanceName;

            try
            {
                // Let caller mutate the profile object
                apply(profile);

                // Handle renaming of profile folder if InstanceName changed
                if (!string.Equals(originalName, profile.InstanceName, StringComparison.OrdinalIgnoreCase))
                {
                    var oldDir = Path.Combine(_ProfileDirectory, originalName);
                    var newDir = Path.Combine(_ProfileDirectory, profile.InstanceName);

                    if (Directory.Exists(newDir))
                    {
                        reason = $"A profile with the name '{profile.InstanceName}' already exists.";
                        // revert change
                        profile.InstanceName = originalName;
                        return false;
                    }

                    if (Directory.Exists(oldDir))
                    {
                        Directory.Move(oldDir, newDir);
                    }
                    else
                    {
                        // If the old directory doesn't exist, create the new directory so file can be saved
                        Directory.CreateDirectory(newDir);
                    }

                    profile.InstancePath = newDir;
                    profile.filePath = Path.Combine(newDir, _instanceCfgFilename);
                }

                // Persist the updated profile
                profile.Save();

                // Notify subscribers
                ProfilesChanged?.Invoke(_instances);

                return true;
            }
            catch (Exception ex)
            {
                reason = ex.Message;
                _logger.Error(ex, $"Failed to update profile '{instanceName}'");
                return false;
            }
        }

        /// <summary>
        /// Attempts to delete the profile identified by the specified instance name.
        /// </summary>
        /// <remarks>If the profile cannot be found or the deletion fails, the reason for failure is
        /// provided in the out parameter. This method does not throw exceptions for user-correctable errors; instead,
        /// it returns false with an appropriate message.</remarks>
        /// <param name="instanceName">The name of the profile instance to delete. Cannot be null, empty, or whitespace.</param>
        /// <param name="reason">When this method returns, contains the reason for failure if the profile could not be deleted; otherwise,
        /// contains an empty string.</param>
        /// <returns>true if the profile was successfully deleted; otherwise, false.</returns>
        public bool TryDeleteProfile(string instanceName, out string reason)
        {
            reason = string.Empty;

            if (string.IsNullOrWhiteSpace(instanceName))
            {
                reason = "Instance name is required.";
                return false;
            }

            if (!TryGetProfileByName(instanceName, out var profile))
            {
                reason = $"Profile '{instanceName}' not found.";
                return false;
            }

            try
            {
                string dir = profile.InstancePath;
                if (string.IsNullOrWhiteSpace(dir))
                {
                    dir = Path.Combine(_ProfileDirectory, profile.InstanceName);
                }

                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }

                _instances.Remove(profile);
                ProfilesChanged?.Invoke(_instances);

                return true;
            }
            catch (Exception ex)
            {
                reason = $"Failed to delete profile '{instanceName}': {ex.Message}";
                _logger.Error(ex, reason);
                return false;
            }
        }

    }
}
