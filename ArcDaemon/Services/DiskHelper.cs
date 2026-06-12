using ArcShared;

namespace ArcDaemon.Services;

public static class DiskHelper
{
    public static string GetDiskInfo()
    {
        return "Disk Info";
    }

    public static List<DriveSummaryPayload> GetDrives()
    {
        DriveInfo[] drives = DriveInfo.GetDrives();
        List<DriveSummaryPayload> payloadList = new List<DriveSummaryPayload>();

        foreach (DriveInfo drive in drives)
        {
            var summary = new DriveSummaryPayload { Name = drive.Name, IsReady = drive.IsReady };

            if (drive.IsReady)
            {
                summary.DriveType = drive.DriveType.ToString();
                summary.FileSystem = drive.DriveFormat;
                summary.VolumeLabel = drive.VolumeLabel;
                summary.AvailableSize = FileSizeFormatter.ToHumanReadable(drive.AvailableFreeSpace);
                summary.TotalFreeSpace = FileSizeFormatter.ToHumanReadable(drive.TotalFreeSpace);
                summary.TotalSize = FileSizeFormatter.ToHumanReadable(drive.TotalSize);
            }

            payloadList.Add(summary);
        }

        return payloadList;
    }

    public class DriveSummaryPayload
    {
        public string Name { get; set; }
        public string DriveType { get; set; }
        public string FileSystem { get; set; }
        public string VolumeLabel { get; set; }
        public string AvailableSize { get; set; }
        public string TotalFreeSpace { get; set; }
        public string TotalSize { get; set; }
        public bool IsReady { get; set; }
    }
}