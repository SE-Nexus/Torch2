using System;
using System.Collections.Generic;
using System.Text;
using Torch2API.Models;

namespace Torch2API.DTOs.Instances
{
    public class TorchInstanceBase : ConfiguredInstance
    {
        public bool IsOnline { get; set; } = true;
        public ServerStatusEnum ServerStatus { get; set; } = ServerStatusEnum.Initializing;
        public ServerStateCommand CurrentStateCmd { get; set; } = ServerStateCommand.Idle;
        public TimeSpan GameUpTime { get; set; }
        public TimeSpan StateTime { get; set; }


        /// <summary>
        /// Updates the current instance's properties to match those of the specified configured instance.
        /// </summary>
        /// <remarks>All properties in the current instance are overwritten with the corresponding values
        /// from the specified instance. This method does not perform a deep copy of reference-type
        /// properties.</remarks>
        /// <param name="cfg">The configured instance from which to copy property values. Cannot be null.</param>
        public void UpdateFromConfiguredInstance(TorchInstanceBase cfg)
        {
            this.InstanceID = cfg.InstanceID;
            this.Name = cfg.Name;
            this.MachineName = cfg.MachineName;
            this.IPAddress = cfg.IPAddress;
            this.GamePort = cfg.GamePort;
            this.ProfileName = cfg.ProfileName;
            this.TargetWorld = cfg.TargetWorld;
            this.TorchVersion = cfg.TorchVersion;
            this.ServerStatus = cfg.ServerStatus;
            this.CurrentStateCmd = cfg.CurrentStateCmd;
            this.GameUpTime = cfg.GameUpTime;
            this.StateTime = cfg.StateTime;
        }

        public string GetFormattedGameUptime()
        {
            var uptime = GameUpTime;

            if (uptime < TimeSpan.Zero)
                uptime = TimeSpan.Zero;

            return $"{uptime.Days:00}:{uptime.Hours:00}:{uptime.Minutes:00}:{uptime.Seconds:00}";
        }


    }
}
