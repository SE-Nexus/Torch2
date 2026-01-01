using System;
using System.Collections.Generic;
using System.Text;
using Torch2API.Models;

namespace Torch2API.DTOs.Instances
{
    public class TorchInstanceBase : ConfiguredInstance
    {
        public ServerStatusEnum ServerStatus { get; set; } = ServerStatusEnum.Initializing;



        public void UpdateFromConfiguredInstance(TorchInstanceBase cfg)
        {
            this.InstanceID = cfg.InstanceID;
            this.Name = cfg.Name;
            this.MachineName = cfg.MachineName;
            this.IPAddress = cfg.IPAddress;
            this.GamePort = cfg.GamePort;
            this.InstanceName = cfg.InstanceName;
            this.TargetWorld = cfg.TargetWorld;
            this.TorchVersion = cfg.TorchVersion;
            ServerStatus = cfg.ServerStatus;
        }


    }
}
