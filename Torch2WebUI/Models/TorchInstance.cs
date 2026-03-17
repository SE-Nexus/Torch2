using Microsoft.VisualBasic;
using Torch2API.DTOs.Instances;
using Torch2API.Models.Configs;
using Torch2API.Models.Schema;
using Torch2API.Models.SE1;

namespace Torch2WebUI.Models
{
    public class TorchInstance : TorchInstanceBase
    {
        public bool Configured { get; set; } = false;

        //Profiles that are saved and recognized by the instance. This is used to display the available profiles in the UI
        public List<ProfileCfg> Profiles { get; set; } = new();

        //Worlds that are saved and recognized by the instance. This is used to display the available worlds in the UI
        public List<WorldInfo> WorldInfos { get; set; } = new();

        //Game custom worlds (Premade) that are saved and recognized by the instance. This is used to display the available custom worlds in the UI
        public List<WorldInfo> CustomWorlds { get; set; } = new();


        // The concrete dedicated config DTO (new): used to receive/send full config objects
        public ConfigDedicatedSE1 DedicatedConfig { get; set; }
    }
}
