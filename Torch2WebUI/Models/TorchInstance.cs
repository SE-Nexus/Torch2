using Torch2API.DTOs.Instances;
using Torch2API.Models.Configs;

namespace Torch2WebUI.Models
{
    public class TorchInstance : TorchInstanceBase
    {
        public List<ProfileCfg> Profiles { get; set; } = new();
    }
}
