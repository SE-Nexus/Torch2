
using IgniteSE1.Configs;
using IgniteSE1.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Services
{
    public class IgniteConfigService : ServiceBase
    {
        private const string _cfgName = "cfg.yml";

        public IgniteSE1Config Config { get; private set; }


        public IgniteConfigService() { }


        public void LoadConfig()
        {
            string fileName = Path.Combine(AppContext.BaseDirectory, _cfgName);
            Config = IgniteSE1Config.LoadYaml(fileName);


        }

    }
}
