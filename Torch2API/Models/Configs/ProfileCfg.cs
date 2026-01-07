using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch2API.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.BufferedDeserialization;

namespace Torch2API.Models.Configs
{
    public class ProfileCfg : ConfigBase<ProfileCfg>
    {
        [YamlIgnore]
        public string InstanceName { get; set; } = "";

        [YamlIgnore]
        public string InstancePath { get; set; } = "";


        [YamlMember(Description = "The last used world in the instance")]
        public string TargetWorld { get; set; } = "NewWorld";

        [YamlMember(Description = "A User Defined Description for this instance")]
        public string Description { get; set; } = "";

        [YamlMember(Description = "Game Instance Port")]
        public ushort InstancePort { get; set; } = 27016;

        [YamlMember(Description = "Auto-Starts the instance when Ignite is Started")]
        public bool AutoStart { get; set; } = false;

        [YamlMember(Description = "Enables checking for game updates")]
        public bool CheckForUpdates { get; set; } = true;

        [YamlMember(Description = "Enables AutoUpdating Game on Server Start")]
        public bool AutoUpdateGame { get; set; } = true;

        [YamlMember(Description = "Automatically Restart that instance on server crash")]
        public bool RestartOnCrash { get; set; } = true;

        [YamlMember(Description = "Target Branch Name")]
        public string BranchName { get; set; } = "public"; // Default branch name, can be changed to "beta" or other branches if needed

        [YamlMember(Description = "Target Branch Password")]
        public string BranchPassword { get; set; } = "";

        [YamlMember(Description = "Max Age for Game Logs in Days. 0 is Infinite")]
        public int LogsMaxAge { get; set; } = 14; // Default is 14 days


        public void Update(ProfileCfg existing)
        {
            this.InstanceName = existing.InstanceName;
            this.InstancePath = existing.InstancePath;
            this.TargetWorld = existing.TargetWorld;
            this.Description = existing.Description;
            this.InstancePort = existing.InstancePort;
            this.AutoStart = existing.AutoStart;
            this.CheckForUpdates = existing.CheckForUpdates;
            this.AutoUpdateGame = existing.AutoUpdateGame;
            this.RestartOnCrash = existing.RestartOnCrash;
            this.BranchName = existing.BranchName;
            this.BranchPassword = existing.BranchPassword;
            this.LogsMaxAge = existing.LogsMaxAge;


        }



    }
}
