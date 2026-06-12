using ArcShared;

namespace ArcDaemon.CommandHandlers;

public class WhoIsHandler : IActionHandler
{
    public async IAsyncEnumerable<ArcMessage> ExecuteAsync(ArcCommand command, CancellationToken cancellationToken)
    {
        yield return new ArcMessage(ArcConstants.MessageTypeResult, command.Id, $"""
             Tool: {command.ArcIdentifier}
             Requested by: {command.Requester}
             Daemon User: {Environment.UserName}
             Daemon Session: {(Environment.UserInteractive ? "DEBUG" : "SERVICE")}
             Machine: {Environment.MachineName}
             """, true);
    }
}