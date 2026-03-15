using IgniteSE1.Services;
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
        public async Task Create([Option("--name", "Profile name")] string name)
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
        public async Task Delete([Option("--name", "Profile name")] string name)
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
            [Option("--name", "Existing profile name")] string name,
            [Option("--newname", "Rename profile to")] string newName = null,
            [Option("--targetworld", "Target world")] string targetWorld = null,
            [Option("--description", "Description")] string description = null,
            [Option("--port", "Instance port")] ushort? instancePort = null,
            [Option("--autostart", "Auto start")] bool? autoStart = null,
            [Option("--checkupdates", "Check for updates")] bool? checkForUpdates = null,
            [Option("--autoupdategame", "Auto update game")] bool? autoUpdateGame = null,
            [Option("--restartoncrash", "Restart on crash")] bool? restartOnCrash = null,
            [Option("--branch", "Branch name")] string branchName = null,
            [Option("--branchpwd", "Branch password")] string branchPwd = null,
            [Option("--logsmaxage", "Logs max age (days)")] int? logsMaxAge = null
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



    }
}
