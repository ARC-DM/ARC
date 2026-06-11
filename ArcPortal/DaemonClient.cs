using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using ArcShared;

namespace ArcPortal;

public class DaemonClient(Action<ArcMessage> onMessage)
{
    public async Task SendCommandAsync(ArcCommand command)
    {
        using var clientStream = new NamedPipeClientStream(".", ArcConstants.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await clientStream.ConnectAsync();
        
        using var reader = new StreamReader(clientStream);
        using var writer = new StreamWriter(clientStream);
        
        writer.AutoFlush = true;
        
        string arcCommand = JsonSerializer.Serialize(command);
        await writer.WriteLineAsync(arcCommand);

        while (true)
        {
            string? json = await reader.ReadLineAsync();
            if (json is null) break;
            ArcMessage? arcMessage = JsonSerializer.Deserialize<ArcMessage>(json);
            if (arcMessage is null) break;
            onMessage(arcMessage);
            if (arcMessage.Type is ArcConstants.MessageTypeResult) break;
        }
    }
}