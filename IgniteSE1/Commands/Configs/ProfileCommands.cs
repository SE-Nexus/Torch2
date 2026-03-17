using IgniteSE1.Services;
using IgniteSE1.Utilities.ModelTransfers;
using InstanceUtils.Services;
using InstanceUtils.Services.Commands.Contexts;
using InstanceUtils.Services.WebPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch2API.Attributes;
using Torch2API.Constants;
using Torch2API.Models.Commands;
using Torch2API.Models.Configs;
using System.Reflection;
using System.Text.Json;
using System.IO;
using Torch2API.Models.SE1;

namespace IgniteSE1.Commands
{
    [CommandGroup("config/profiles", "Commands to manage the Instance Profiles", CommandTypeEnum.AdminOnly)]
    public class ProfileCommands
    {
        private ICommandContext ctx;
        private ProfileManager _InstanceManager;
        private PanelHTTPClient _PanelClient;

        public ProfileCommands(ICommandContext ctxICommandContext, ProfileManager iManager, PanelHTTPClient webPanelClient)
        {
            this.ctx = ctxICommandContext;
            _InstanceManager = iManager;
            _PanelClient = webPanelClient;
        }


        [Command("list", "Displays all the instance profiles")]
        public async Task List()
        {
            var instances = _InstanceManager.GetAllInstances();

            if(ctx is WebPanelContext)
            {
                await _PanelClient.PostAsync(WebAPIConstants.AllProfiles, _InstanceManager.GetAllInstances());
                return;
            }

            ctx.RespondLine("All Instances:");
            foreach(var instance in instances)
            {
                ctx.RespondLine($" - {instance.InstanceName}");
            }
        }

        [Command("create", "Creates a new profile")]
        public async Task Create([Option("name", "Profile name")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ctx.RespondLine("A profile name is required.");
                return;
            }

            var (ok, msg) = _InstanceManager.TryCreateNewProfile(name, out var cfg);
            if (!ok)
            {
                ctx.RespondLine($"Failed to create profile: {msg}");
                return;
            }

            ctx.RespondLine($"Successfully created profile '{cfg.InstanceName}'.");

            if (ctx is WebPanelContext)
            {
                await _PanelClient.PostAsync(WebAPIConstants.AllProfiles, _InstanceManager.GetAllInstances());
            }
        }

        [Command("delete", "Deletes the specified profile")]
        public async Task Delete([Option("name", "Profile name")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ctx.RespondLine("A profile name is required.");
                return;
            }

            if (!_InstanceManager.TryDeleteProfile(name, out var reason))
            {
                ctx.RespondLine(reason);
                return;
            }

            ctx.RespondLine($"Successfully deleted profile '{name}'.");

            if (ctx is WebPanelContext)
            {
                await _PanelClient.PostAsync(WebAPIConstants.AllProfiles, _InstanceManager.GetAllInstances());
            }
        }

        [Command("update", "Updates a profile. Provide only the fields you want to change.")]
        public async Task Update(
            [Option("name", "Existing profile name")] string name,
            [Option("newname", "Rename profile to")] string newName = null,
            [Option("targetworld", "Target world")] string targetWorld = null,
            [Option("description", "Description")] string description = null,
            [Option("port", "Instance port")] ushort? instancePort = null,
            [Option("autostart", "Auto start")] bool? autoStart = null,
            [Option("checkupdates", "Check for updates")] bool? checkForUpdates = null,
            [Option("autoupdategame", "Auto update game")] bool? autoUpdateGame = null,
            [Option("restartoncrash", "Restart on crash")] bool? restartOnCrash = null,
            [Option("branch", "Branch name")] string branchName = null,
            [Option("branchpwd", "Branch password")] string branchPwd = null,
            [Option("logsmaxage", "Logs max age (days)")] int? logsMaxAge = null
        )
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ctx.RespondLine("Existing profile name is required.");
                return;
            }

            bool result = _InstanceManager.TryApplyProfileUpdate(name, profile =>
            {
                // Apply only provided values
                if (!string.IsNullOrWhiteSpace(newName))
                    profile.InstanceName = newName;

                if (!string.IsNullOrWhiteSpace(targetWorld))
                    profile.TargetWorld = targetWorld;

                if (!string.IsNullOrWhiteSpace(description))
                    profile.Description = description;

                if (instancePort.HasValue && instancePort.Value != 0)
                    profile.InstancePort = instancePort.Value;

                if (autoStart.HasValue)
                    profile.AutoStart = autoStart.Value;

                if (checkForUpdates.HasValue)
                    profile.CheckForUpdates = checkForUpdates.Value;

                if (autoUpdateGame.HasValue)
                    profile.AutoUpdateGame = autoUpdateGame.Value;

                if (restartOnCrash.HasValue)
                    profile.RestartOnCrash = restartOnCrash.Value;

                if (!string.IsNullOrWhiteSpace(branchName))
                    profile.BranchName = branchName;

                if (!string.IsNullOrWhiteSpace(branchPwd))
                    profile.BranchPassword = branchPwd;

                if (logsMaxAge.HasValue)
                    profile.LogsMaxAge = logsMaxAge.Value;

            }, out var reason);

            if (!result)
            {
                ctx.RespondLine($"Failed to update profile: {reason}");
                return;
            }

            ctx.RespondLine($"Successfully updated profile '{newName ?? name}'.");

            if (ctx is WebPanelContext)
            {
                await _PanelClient.PostAsync(WebAPIConstants.AllProfiles, _InstanceManager.GetAllInstances());
            }
        }

        [Command("load", "Sets the specified profile as the configured/active profile")]
        public async Task Load([Option("name", "Profile name")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ctx.RespondLine("A profile name is required.");
                return;
            }

            if (!_InstanceManager.TryLoadProfile(name, out var reason))
            {
                ctx.RespondLine($"Failed to load profile: {reason}");
                return;
            }

            ctx.RespondLine($"Successfully loaded profile '{name}'.");

            if (ctx is WebPanelContext)
            {
                await _PanelClient.PostAsync(WebAPIConstants.AllProfiles, _InstanceManager.GetAllInstances());
            }
        }



        [Command("dedicatedcfg", "Gets and sets the Games Dedicated Cfg Schema")]
        public async Task DedicatedCfgSchema(
            [Option("set", "Setting name to set (optional)")] string set = null,
            [Option("value", "Value to assign to the setting (required when --set provided)")] string value = null,
            [Option("model", "JSON string of full ConfigDedicatedSE1 or '@path' to file (optional)")] string model = null,
            [Option("list", "If set, print all valid ConfigDedicatedSE1 setting names and exit")] bool list = false,
            [Option("profile", "Profile name to load dedicated config from (optional)")] string profile = null)
        {
            // If list requested, ensure no other inputs conflict and print the available setting names
            if (list)
            {
                if (!string.IsNullOrWhiteSpace(set) || !string.IsNullOrWhiteSpace(model) || !string.IsNullOrWhiteSpace(value))
                {
                    ctx.RespondLine("When using --list no other options should be provided.");
                    return;
                }

                var propNames = typeof(ConfigDedicatedSE1)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => p.Name)
                    .OrderBy(n => n)
                    .ToList();

                ctx.RespondLine($"Valid ConfigDedicatedSE1 settings ({propNames.Count}):");
                foreach (var n in propNames)
                    ctx.RespondLine($" - {n}");

                return;
            }

            // Prevent ambiguous input between single-set and model
            if (!string.IsNullOrWhiteSpace(set) && !string.IsNullOrWhiteSpace(model))
            {
                ctx.RespondLine("Provide either --set/--value to change a single property or --model to provide the full model, not both.");
                return;
            }


          
            // Load the runtime IMyConfigDedicated for either the provided profile or the current profile
            var runtimeCfg = default(object);
            if (!string.IsNullOrWhiteSpace(profile))
            {
                if (!_InstanceManager.TryGetProfileByName(profile, out var profileCfg))
                {
                    ctx.RespondLine($"Profile '{profile}' not found.");
                    return;
                }

                runtimeCfg = _InstanceManager.GetServerConfigs(profileCfg);
            }
            else
            {
                runtimeCfg = _InstanceManager.GetServerConfigs();
            }

            // If a full model JSON was provided, parse it
            ConfigDedicatedSE1 modelObj = ConfigModelTransfer.GetDedicatedConfig((dynamic)runtimeCfg);
            if (!string.IsNullOrWhiteSpace(model))
            {
                //File path support: if model starts with @ treat the rest as file path and try to read JSON from it
                string json = model!;
                if (json.StartsWith("@"))
                {
                    var path = json.Substring(1).Trim();
                    if (!File.Exists(path))
                    {
                        ctx.RespondLine($"Model file not found: {path}");
                        return;
                    }

                    try
                    {
                        json = File.ReadAllText(path);
                    }
                    catch (Exception ex)
                    {
                        ctx.RespondLine($"Failed to read model file: {ex.Message}");
                        return;
                    }
                }

                try
                {
                    //Try to deserialize the JSON into the ConfigDedicatedSE1 model
                    modelObj = JsonSerializer.Deserialize<ConfigDedicatedSE1>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (modelObj == null)
                    {
                        ctx.RespondLine("Failed to deserialize model JSON to ConfigDedicatedSE1.");
                        return;
                    }

                    ctx.RespondLine("Successfully parsed model JSON.");
                }
                catch (Exception ex)
                {
                    ctx.RespondLine($"Failed to parse model JSON: {ex.Message}");
                    return;
                }
            }

            // If a single setting is provided, try to set it on the DTO before sending/applying
            if (!string.IsNullOrWhiteSpace(set))
            {
                if (value is null)
                {
                    ctx.RespondLine("When using --set you must also provide --value.");
                    return;
                }

                var prop = typeof(ConfigDedicatedSE1)
                    .GetProperty(set, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (prop == null)
                {
                    ctx.RespondLine($"Unknown setting '{set}'.");
                    return;
                }

                try
                {
                    // Convert to property type (basic types). Complex types (lists) may require JSON parsing.
                    object converted;
                    if (prop.PropertyType == typeof(string))
                    {
                        converted = value;
                    }
                    else if (prop.PropertyType.IsEnum)
                    {
                        converted = Enum.Parse(prop.PropertyType, value, ignoreCase: true);
                    }
                    else if (prop.PropertyType == typeof(List<string>))
                    {
                        // try parse as comma-separated or JSON array
                        if (value.TrimStart().StartsWith("["))
                            converted = JsonSerializer.Deserialize<List<string>>(value);
                        else
                            converted = value.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
                    }
                    else if (prop.PropertyType == typeof(List<ulong>))
                    {
                        if (value.TrimStart().StartsWith("["))
                            converted = JsonSerializer.Deserialize<List<ulong>>(value);
                        else
                            converted = value.Split(',').Select(s => ulong.Parse(s.Trim())).ToList();
                    }
                    else
                    {
                        converted = Convert.ChangeType(value, prop.PropertyType);
                    }

                    prop.SetValue(modelObj, converted);
                    ctx.RespondLine($"Applied setting '{set}' = '{value}' to runtime config.");
                }
                catch (Exception)
                {
                    ctx.RespondLine($"Failed to convert '{value}' to {prop.PropertyType.Name}.");
                    return;
                }
            }

            // If web panel context, post either the provided full model or the constructed DTO
            await _PanelClient.PostAsync(WebAPIConstants.DedicatedSchema, modelObj);

            // Apply the model back into the runtime dedicated config we previously retrieved
            var cfgDedicated = (dynamic)runtimeCfg ?? _InstanceManager.GetServerConfigs();
            ConfigModelTransfer.SetDedicatedConfig(cfgDedicated, modelObj);
            cfgDedicated.Save();
        }
    }
}
