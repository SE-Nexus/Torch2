using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace InstanceUtils.Services.Networking
{
    public class NamedPipeCommandClient
    {
        private const string PipeNamePrefix = "ignite-cli-pipe-";
        private readonly string _pipeName;

        public NamedPipeCommandClient(Guid instanceId)
        {
            _pipeName = PipeNamePrefix + instanceId.ToString("N");
        }

        public async Task<string> SendCommandAsync(string[] command)
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut))
                {
                    await client.ConnectAsync(5000);

                    // Send request
                    var request = new CliMessage { Command = command };
                    byte[] requestData = request.Serialize();
                    byte[] requestLength = BitConverter.GetBytes(requestData.Length);

                    await client.WriteAsync(requestLength, 0, 4);
                    await client.WriteAsync(requestData, 0, requestData.Length);
                    await client.FlushAsync();

                    // Read response length
                    byte[] lengthBuffer = new byte[4];
                    int bytesRead = await client.ReadAsync(lengthBuffer, 0, 4);
                    if (bytesRead < 4)
                        throw new Exception("Failed to read response length");

                    int responseLength = BitConverter.ToInt32(lengthBuffer, 0);
                    if (responseLength <= 0)
                        throw new Exception("Invalid response length");

                    // Read response
                    byte[] responseData = new byte[responseLength];
                    bytesRead = await client.ReadAsync(responseData, 0, responseLength);
                    var response = CliMessage.Deserialize(responseData);

                    return response.Result;
                }
            }
            catch (TimeoutException)
            {
                throw new Exception("Failed to connect to the main instance. Is it running?");
            }
            catch (IOException ex)
            {
                throw new Exception($"Communication error with main instance: {ex.Message}");
            }
        }
    }
}
