using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ArcDaemon.Services;
using ArcShared;

namespace ArcDaemon.CommandHandlers;

public class ProcessesHandler : IActionHandler
{
    public async IAsyncEnumerable<ArcMessage> ExecuteAsync(ArcCommand command, CancellationToken cancellationToken)
    {
        string result = ProcessHelper.GetTopProcessesFormatted(5);
        yield return new ArcMessage(ArcConstants.MessageTypeResult, command.Id, result, true);
    }
}