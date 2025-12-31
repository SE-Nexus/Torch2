using Torch2API.DTOs.Instances;

namespace Torch2WebUI.Services.SQL
{
    public class TorchInstance : ITorchInstanceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string InstanceID { get; set; } = string.Empty;
    }
}
