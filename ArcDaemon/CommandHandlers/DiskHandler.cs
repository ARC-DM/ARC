using System.Runtime.CompilerServices;
using ArcDaemon.Services;
using ArcShared;

namespace ArcDaemon.CommandHandlers;

public class DiskHandler : IActionHandler
{
    public async IAsyncEnumerable<ArcMessage> ExecuteAsync(ArcCommand command,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        List<DiskHelper.DriveSummaryPayload> driveData = DiskHelper.GetDrives();

        var driveLines = driveData.Select(drive =>
        {
            if (!drive.IsReady)
            {
                return $"""
                        Drive {drive.Name}
                            Status: Not Ready
                        """;
            }

            return $"""
                    Drive {drive.Name}
                        Status: Ready
                        Type: {drive.DriveType}
                        File System: {drive.FileSystem}
                        Volume Label: {drive.VolumeLabel}
                        Available Size: {drive.AvailableSize}
                        Total Free Space: {drive.TotalFreeSpace}
                        Total Size: {drive.TotalSize}
                    """;
        });

        string payload = string.Join($"{Environment.NewLine}{Environment.NewLine}", driveLines);

        yield return new ArcMessage(ArcConstants.MessageTypeResult, command.Id, payload, true);
    }
}