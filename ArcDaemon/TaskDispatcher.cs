using System.Runtime.CompilerServices;
using ArcDaemon.CommandHandlers;
using ArcShared;

namespace ArcDaemon;

public class TaskDispatcher
{
    private Dictionary<string, IActionHandler> ActionHandlers { get; } = new();

    // Expose the keys so the HelpHandler can discover what is registered
    public IEnumerable<string> RegisteredActions => ActionHandlers.Keys;

    public void RegisterActionHandler(string action, IActionHandler handler)
    {
        ActionHandlers.Add(action, handler);
    }

    public async IAsyncEnumerable<ArcMessage> DispatchAsync(ArcCommand command,
        [EnumeratorCancellation] CancellationToken ct)
    {
        if (ActionHandlers.TryGetValue(command.Action, out var handler))
        {
            // Stream the results from the handler back to the caller
            await foreach (var message in handler.ExecuteAsync(command, ct))
            {
                yield return message;
            }
        }
        else
        {
            yield return new ArcMessage(ArcConstants.MessageTypeResult, command.Id, $"Unknown action: {command.Action}",
                false);
        }
    }
}