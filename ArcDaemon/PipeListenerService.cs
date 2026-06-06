using System.IO.Pipes;
using ArcShared;

namespace ArcDaemon;

public class PipeListenerService(ILogger<PipeListenerService> logger, ConnectionHandler connectionHandler) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            NamedPipeServerStream serverStream = new NamedPipeServerStream(ArcConstants.PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            
            await serverStream.WaitForConnectionAsync(stoppingToken);
            _ = Task.Run(() => connectionHandler.HandleAsync(serverStream, stoppingToken));
        }
    }
}