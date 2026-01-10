using Microsoft.VisualBasic;
using Torch2API.DTOs.Instances;
using Torch2API.Models.Configs;

namespace Torch2WebUI.Models
{
    public class TorchInstance : TorchInstanceBase
    {
        public bool Configured { get; set; } = false;

        public List<ProfileCfg> Profiles { get; set; } = new();

        //Saved worlds
        public List<WorldInfo> WorldInfos { get; set; } = new();

        //World templates
        public List<WorldInfo> CustomWorlds { get; set; } = new();
    }
}
