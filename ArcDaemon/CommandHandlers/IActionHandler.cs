using ArcShared;

namespace ArcDaemon.CommandHandlers;

public interface IActionHandler
{
    IAsyncEnumerable<ArcMessage> ExecuteAsync(ArcCommand command, CancellationToken cancellationToken);
}