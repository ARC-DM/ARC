using System.Runtime.InteropServices;
using System.Text.Json;
using System.Management;
using ArcShared;

namespace ArcDaemon.CommandHandlers;

public class DiskHandler : IActionHandler
{
    public async IAsyncEnumerable<ArcMessage> ExecuteAsync(ArcCommand command, CancellationToken cancellationToken)
    {
        DriveInfo[] drives = DriveInfo.GetDrives();

        foreach (DriveInfo drive in drives)
        {
            yield return new ArcMessage(ArcConstants.MessageTypeProgress, command.Id, "Drive " + drive.Name, true);
            yield return new ArcMessage(ArcConstants.MessageTypeProgress, command.Id,
                "Drive Type: " + drive.DriveType.ToString(), true);
            yield return new ArcMessage(ArcConstants.MessageTypeProgress, command.Id,
                "File System: " + drive.DriveFormat, true);
            yield return new ArcMessage(ArcConstants.MessageTypeProgress, command.Id,
                "Volume Label: " + drive.VolumeLabel, true);
            yield return new ArcMessage(ArcConstants.MessageTypeProgress, command.Id,
                "Available space to current user: " + FileSizeFormatter.ToHumanReadable(drive.AvailableFreeSpace), true);
            yield return new ArcMessage(ArcConstants.MessageTypeProgress, command.Id,
                "Total available space: " + FileSizeFormatter.ToHumanReadable(drive.TotalFreeSpace), true);
            yield return new ArcMessage(ArcConstants.MessageTypeProgress, command.Id,
                "Total Size of Drive: " + FileSizeFormatter.ToHumanReadable(drive.TotalSize), true);
        }

        yield return new ArcMessage(ArcConstants.MessageTypeResult, command.Id, "Done", true);
    }

    private static class FileSizeFormatter
    {
        private static readonly string[] SizeSuffixes =
            { "bytes", "KB", "MB", "GB", "TB", "PB", "EB" };

        public static string ToHumanReadable(long bytes, int decimalPlaces = 2)
        {
            // Validate decimalPlaces
            if (decimalPlaces < 0)
                throw new ArgumentOutOfRangeException(nameof(decimalPlaces), "Decimal places must be non-negative.");

            // Handle negative values
            if (bytes < 0)
                return "-" + ToHumanReadable(-bytes, decimalPlaces);

            // Bytes less than 1 KB
            if (bytes < 1024)
                return $"{bytes} {SizeSuffixes[0]}";

            int mag = (int)Math.Log(bytes, 1024); // Determine magnitude
            decimal adjustedSize = (decimal)bytes / (1L << (mag * 10));

            // Ensure rounding does not push value to next unit
            if (Math.Round(adjustedSize, decimalPlaces) >= 1024 && mag < SizeSuffixes.Length - 1)
            {
                mag++;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
        }
    }
}