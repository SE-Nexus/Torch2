using IgniteSE1.Configs;
using IgniteUtils.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Services
{
    public class ConfigService : ServiceBase
    {
        private const string _cfgName = "cfg.yml";

        public IgniteSE1Cfg Config { get; private set; }


        public ConfigService() { }


        public void LoadConfig()
        {
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
