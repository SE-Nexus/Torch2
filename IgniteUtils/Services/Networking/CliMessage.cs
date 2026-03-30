using InstanceUtils.Utils;

namespace InstanceUtils.Services.Networking
{
    public class CliMessage
    {
        public string[] Command { get; set; }
        public string Result { get; set; }

        public byte[] Serialize() => SerializationUtils.ToJson(this);

        public static CliMessage Deserialize(byte[] data) => SerializationUtils.FromJson<CliMessage>(data);
    }
}
