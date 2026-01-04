using IgniteSE1.Configs;
using InstanceUtils.Models.Server;
using InstanceUtils.Services;
using InstanceUtils.Utils.Identification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Services
{
    //This can be cleaned up
    public class ConfigService : ServiceBase, IConfigService
    {
        private const string _cfgName = "cfg.yml";


        public IgniteSE1Cfg Config { get; private set; }

        
        public InstanceIdentification Identification { get; private set; }

        public string InstanceName => Config.IgniteCMDName;

        public Uri TargetWebPanel => new Uri(Config.WebServerAddress);

        public string SteamCMDPath => Config.Directories.SteamCMDFolder;

        public string GamePath => Config.Directories.Game;



        public ConfigService() { }


        public void LoadConfig()
        {
            //Load the identification file
            Identification = new InstanceIdentification(AppContext.BaseDirectory);
            Identification.Initialize();

            string fileName = Path.Combine(AppContext.BaseDirectory, _cfgName);
            Config = IgniteSE1Cfg.LoadYaml(fileName);


            //Ensure directories exist
            if (!Directory.Exists(Config.Directories.ModStorage))
                Directory.CreateDirectory(Config.Directories.ModStorage);

            if (!Directory.Exists(Config.Directories.Instances))
                Directory.CreateDirectory(Config.Directories.Instances);

            if (!Directory.Exists(Config.Directories.Game))
                Directory.CreateDirectory(Config.Directories.Game);

            if (!Directory.Exists(Config.Directories.SteamCMDFolder))
                Directory.CreateDirectory(Config.Directories.SteamCMDFolder);
        }

        public override async Task<bool> Init()
        {
            //Test timeout
            await Task.Delay(2000);
            return true;
        }

    }
}
