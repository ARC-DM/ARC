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

        yield return new ArcMessage(ArcConstants.MessageTypeProgress, command.Id, "Starting installation...", true);

        var channel = Channel.CreateUnbounded<string>();

        // Set up arguments based on priority (ID takes precedence)
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

        // Stream lines as they arrive
        await foreach (var line in channel.Reader.ReadAllAsync(cancellationToken))
            yield return new ArcMessage(ArcConstants.MessageTypeProgress, command.Id, line, true);

        await process.WaitForExitAsync(cancellationToken);

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