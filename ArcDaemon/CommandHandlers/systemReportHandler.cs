using ArcDaemon.Services;
using ArcShared;

namespace ArcDaemon.CommandHandlers;

public class systemReportHandler : IActionHandler
{
    public async IAsyncEnumerable<ArcMessage> ExecuteAsync(ArcCommand command, CancellationToken cancellationToken)
    {
        yield return new ArcMessage(ArcConstants.MessageTypeProgress, command.Id, "Generating System Report...", true);
    
        string processData = ProcessHelper.GetTopProcessesFormatted(5);
    
        yield return new ArcMessage(ArcConstants.MessageTypeResult, command.Id, $"""
             --- SYSTEM REPORT ---
             --- PROCESSES ---
             {processData}
             """, true);
    }
}