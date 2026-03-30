using InstanceUtils.Services;
using InstanceUtils.Services.Networking;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace IgniteSE1.Services.Networking
{
    public class NamedPipeCommandServer
    {
        private const string PipeNamePrefix = "ignite-cli-pipe-";
        private readonly CommandService _commandService;
        private readonly string _pipeName;
        private CancellationTokenSource _cancellationTokenSource;

        public NamedPipeCommandServer(CommandService commandService, Guid instanceId)
        {
            _commandService = commandService;
            _pipeName = PipeNamePrefix + instanceId.ToString("N");
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => ListenForConnectionsAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }

        private async Task ListenForConnectionsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                NamedPipeServerStream server = null;
                try
                {
                    server = new NamedPipeServerStream(_pipeName, PipeDirection.InOut);
                    await server.WaitForConnectionAsync(cancellationToken);

                    try
                    {
                        // Read request length (4 bytes)
                        byte[] lengthBuffer = new byte[4];
                        int bytesRead = await server.ReadAsync(lengthBuffer, 0, 4, cancellationToken);
                        if (bytesRead < 4)
                            continue;

                        int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                        if (messageLength <= 0)
                            continue;

                        // Read request JSON
                        byte[] messageBuffer = new byte[messageLength];
                        bytesRead = await server.ReadAsync(messageBuffer, 0, messageLength, cancellationToken);
                        var request = CliMessage.Deserialize(messageBuffer);

                        // Execute command
                        string result = await _commandService.InvokeCLICommand(request.Command);

                        // Send response
                        var response = new CliMessage { Result = result };
                        byte[] responseData = response.Serialize();
                        byte[] responseLength = BitConverter.GetBytes(responseData.Length);

                        await server.WriteAsync(responseLength, 0, 4, cancellationToken);
                        await server.WriteAsync(responseData, 0, responseData.Length, cancellationToken);
                        await server.FlushAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Named Pipe Handler Error] {ex.Message}");
                        try
                        {
                            var errorResponse = new CliMessage { Result = $"Error: {ex.Message}" };
                            byte[] errorData = errorResponse.Serialize();
                            byte[] errorLength = BitConverter.GetBytes(errorData.Length);
                            await server.WriteAsync(errorLength, 0, 4);
                            await server.WriteAsync(errorData, 0, errorData.Length);
                            await server.FlushAsync();
                        }
                        catch { }
                    }

                    try
                    {
                        server.Disconnect();
                    }
                    catch (ObjectDisposedException) { }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Named Pipe Server Error] {ex}");
                }
                finally
                {
                    server?.Dispose();
                }
            }
        }
    }
}
