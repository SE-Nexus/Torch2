using System;
using System.Text.Json;

namespace InstanceUtils.Utils
{
    public static class SerializationUtils
    {
        public static byte[] ToJson<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public static T FromJson<T>(byte[] data)
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
