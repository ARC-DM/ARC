using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using ArcShared;

namespace ArcDaemon.CommandHandlers;

public class InstallHandler : IActionHandler
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

        yield return new ArcMessage(ArcConstants.MessageTypeProgress, command.Id, "Starting installation...", true);

        // 1. Create a Channel to pipe stdout/stderr back as progress messages
        var channel = Channel.CreateUnbounded<string>();

        var arguments = !string.IsNullOrEmpty(appId)
            ? $"install -e --id {appId} --silent --accept-package-agreements --accept-source-agreements"
            : $"install {appName} --silent --accept-package-agreements --accept-source-agreements";

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

        // 2. Start a background task to safely await the process exit and close the channel
        var processWaitTask = Task.Run(async () =>
        {
            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            finally
            {
                // This signals to Channel.Reader that no more logs are coming
                channel.Writer.TryComplete();
            }
        }, cancellationToken);

        // 4. Ensure the process tracking task itself is fully wrapped up
        await processWaitTask;

        // 5. Now it is 100% safe to send the final result
        yield return new ArcMessage(
            ArcConstants.MessageTypeResult,
            command.Id,
            process.ExitCode == 0
                ? "Installation finished successfully."
                : $"Installation failed with code {process.ExitCode}.",
            process.ExitCode == 0
        );
    }
}