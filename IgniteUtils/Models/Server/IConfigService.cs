using IgniteUtils.Utils.Identification;
using System;
using System.Collections.Generic;
using System.Text;

namespace IgniteUtils.Models.Server
{
    //Interface for the config service
    public interface IConfigService
    {
        public InstanceIdentification Identification { get; }

        public Uri TargetWebPanel { get; }

        public string InstanceName { get; }

        public string SteamCMDPath { get; }

        public string GamePath { get; }

    }
}
