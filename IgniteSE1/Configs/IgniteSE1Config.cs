using IgniteSE1.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace IgniteSE1.Configs
{
    public class IgniteSE1Config : ConfigBase<IgniteSE1Config>
    {

        #region Yaml Groups
        public class DirectoriesConfig
        {

            [YamlMember(Description = "Target Instance to Load")]
            public string SteamCMDFolder { get; set; } = "SteamCMD";

            [YamlMember(Description = "Path to Installed Game")]
            public string Game { get; set; } = "Game";

            [YamlMember(Description = "Path to Mod Storage")]
            public string ModStorage { get; set; } = "Mods";
        }

        #endregion



        [YamlMember(Description = "Changes the name of the CMD Window")]
        public string IgniteCMDName { get; set; } = "Ignite SE1";


        [YamlMember(Description = "Auto Starts the Server")]
        public bool AutoStartServer { get; set; } = true;


        [YamlMember(Description = "Directories Configuration")]
        public DirectoriesConfig Directories { get; set; } = new DirectoriesConfig();




    }
}
