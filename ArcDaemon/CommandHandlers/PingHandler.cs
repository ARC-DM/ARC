using System.Runtime.CompilerServices;
using ArcShared;

namespace ArcDaemon.CommandHandlers;

public class PingHandler : IActionHandler
{
    public async IAsyncEnumerable<ArcMessage> ExecuteAsync(ArcCommand command,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);

        var payload = $"""
                       Machine Name: {Environment.MachineName}
                       User Name: {Environment.UserName}
                       OS: {Environment.OSVersion}
                       Uptime: {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s
                       .NET Version: {Environment.Version}
                       """;

        // Return the final result
        yield return new ArcMessage(ArcConstants.MessageTypeResult, command.Id, payload, true);
    }
}