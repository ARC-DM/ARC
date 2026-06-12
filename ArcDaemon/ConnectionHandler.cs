using System.IO.Pipes;
using System.Text.Json;
using ArcShared;

namespace ArcDaemon;

public class ConnectionHandler(ILogger<ConnectionHandler> logger, TaskDispatcher taskDispatcher)
{
    public async Task HandleAsync(NamedPipeServerStream pipeStream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(pipeStream);
        using var writer = new StreamWriter(pipeStream) { AutoFlush = true };

        string? json = await reader.ReadLineAsync();
        if (json is null) return;

        ArcCommand? arcCommand = JsonSerializer.Deserialize<ArcCommand>(json);
        if (arcCommand is null) return;
        
        // Check if user exists in database
        if (AuthenticationHandler.DoesUserExist(arcCommand.Requester) == false)
        {
            logger.LogInformation("User {username} not found", arcCommand.Requester);
            // Create user since they dont exist
            AuthenticationHandler.AddUser(arcCommand.Requester, "NONE");
        }

        logger.LogInformation("Received command: {Id} - {Command} | {Requester} {FrontIdentifier}", arcCommand.Id, arcCommand.Action, arcCommand.Requester, arcCommand.ArcIdentifier);

        // 1. Initial "Command Received" message
        var initialMessage = new ArcMessage(ArcConstants.MessageTypeCallback, arcCommand.Id, "Command Received", false);
        await writer.WriteLineAsync(JsonSerializer.Serialize(initialMessage));

        // 2. Dispatch and Stream responses
        // Assumption: DispatchAsync now returns IAsyncEnumerable<ArcMessage>
        await foreach (var message in taskDispatcher.DispatchAsync(arcCommand, cancellationToken))
        {
            try
            {
                await writer.WriteLineAsync(JsonSerializer.Serialize(message));
            }
            catch (IOException ex)
            {
                logger.LogError(ex, "Client disconnected while sending progress.");
                break;
            }
        }
    }
}