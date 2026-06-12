using System.Runtime.CompilerServices;
using ArcShared;

namespace ArcDaemon.CommandHandlers;

public class HelpHandler : IActionHandler
{
    private readonly TaskDispatcher _dispatcher;

    // Inject the dispatcher to read the live registry
    public HelpHandler(TaskDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async IAsyncEnumerable<ArcMessage> ExecuteAsync(
        ArcCommand command,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return new ArcMessage(ArcConstants.MessageTypeProgress, command.Id, "Available system commands:", true);

        foreach (var action in _dispatcher.RegisteredActions)
        {
            // Map descriptions cleanly based on the action string constant
            string description = action switch
            {
                ArcConstants.ActionPing => "Ping the daemon to verify connectivity.",
                ArcConstants.ActionInstall => "Install an application using winget. (Params: app, id)",
                ArcConstants.ActionUninstall => "Uninstall an application using winget. (Params: app, id)",
                ArcConstants.ActionWhoAmI => "Display the user account context the daemon is executing under.",
                ArcConstants.ActionProcesses => "List active processes running on the host system.",
                ArcConstants.ActionSystemReport => "Generate a summary report of host hardware and health.",
                ArcConstants.ActionDrives => "List available storage drives and free space details.",
                ArcConstants.ActionHelp => "Display this help utility catalog.",
                _ => "No documentation available for this action."
            };

            // Format it nicely for your monospace web view layout
            string helpLine = $"  {action,-15} - {description}";
            yield return new ArcMessage(ArcConstants.MessageTypeProgress, command.Id, helpLine, true);
        }

        // Send the completion result so your front-end spinner resolves to a checkmark
        yield return new ArcMessage(ArcConstants.MessageTypeResult, command.Id, null, true);
    }
}