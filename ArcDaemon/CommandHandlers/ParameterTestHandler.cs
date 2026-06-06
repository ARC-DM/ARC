using System.Text.Json;
using ArcShared;

namespace ArcDaemon.CommandHandlers;

public class ParameterTestHandler : IActionHandler
{
    public async IAsyncEnumerable<ArcMessage> ExecuteAsync(ArcCommand command, CancellationToken cancellationToken)
    {
        yield return new ArcMessage(ArcConstants.MessageTypeResult, command.Id,
            $"Sent with parameters: {JsonSerializer.Serialize(command.Parameters)}", true);
    }
}