using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using ArcShared;

namespace ArcDaemon.CommandHandlers;

public class UninstallHandler : IActionHandler
{
    public async IAsyncEnumerable<ArcMessage> ExecuteAsync(
        ArcCommand command,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        command.Parameters.TryGetValue("app", out var appName);
        command.Parameters.TryGetValue("id", out var appId);

        if (string.IsNullOrEmpty(appName) && string.IsNullOrEmpty(appId))
        {
            yield return new ArcMessage(ArcConstants.MessageTypeResult, command.Id,
                "Error: Missing 'app' or 'id' parameter.", false);
            yield break;
        }

        yield return new ArcMessage(ArcConstants.MessageTypeProgress, command.Id, "Starting uninstall...", true);

        var channel = Channel.CreateUnbounded<string>();

        var arguments = !string.IsNullOrEmpty(appId)
            ? $"uninstall -e --id {appId} --silent --accept-source-agreements"
            : $"uninstall {appName} --silent --accept-source-agreements";

        var psi = new ProcessStartInfo("winget")
        {
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };

        process.OutputDataReceived += (s, e) =>
        {
            if (e.Data != null) channel.Writer.TryWrite(e.Data);
        };
        process.ErrorDataReceived += (s, e) =>
        {
            if (e.Data != null) channel.Writer.TryWrite("ERROR: " + e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // 1. Kick off a background task to wait for exit and close the channel writer
        var processWaitTask = Task.Run(async () =>
        {
            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            finally
            {
                // This breaks out of the 'await foreach' loop below
                channel.Writer.TryComplete();
            }
        }, cancellationToken);

        // 3. Await the process tracking task to ensure everything cleanly wrapped up
        await processWaitTask;

        // 4. Send your final result
        yield return new ArcMessage(
            ArcConstants.MessageTypeResult,
            command.Id,
            process.ExitCode == 0
                ? "Uninstall finished successfully."
                : $"Uninstallation failed with code {process.ExitCode}.",
            process.ExitCode == 0
        );
    }
}