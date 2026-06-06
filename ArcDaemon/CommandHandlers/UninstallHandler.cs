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
        // Try to get either parameter
        command.Parameters.TryGetValue("app", out var appName);
        command.Parameters.TryGetValue("id", out var appId);

        // If both are null/empty, we can't proceed
        if (string.IsNullOrEmpty(appName) && string.IsNullOrEmpty(appId))
        {
            yield return new ArcMessage(ArcConstants.MessageTypeResult, command.Id,
                "Error: Missing 'app' or 'id' parameter.", false);
            yield break;
        }

        yield return new ArcMessage(ArcConstants.MessageTypeProgress, command.Id, "Starting uninstall...", true);

        var channel = Channel.CreateUnbounded<string>();

        // Set up arguments based on priority (ID takes precedence)
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

        // Stream lines as they arrive
        await foreach (var line in channel.Reader.ReadAllAsync(cancellationToken))
            yield return new ArcMessage(ArcConstants.MessageTypeProgress, command.Id, line, true);

        await process.WaitForExitAsync(cancellationToken);

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