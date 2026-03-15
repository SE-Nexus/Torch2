using Microsoft.VisualBasic;
using Torch2API.DTOs.Instances;
using Torch2API.Models.Configs;
using Torch2API.Models.Schema;

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

        //The schema for the dedicated config. This is used to generate the config UI and validate config changes
        public List<SettingDefinition> DedicatedSchema { get; set; } = new();
    }
}
