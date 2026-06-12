namespace ArcShared;

public class FileSizeFormatter
{
    private static readonly string[] Suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

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
            return $"{bytes} {Suffixes[0]}";

        int mag = (int)Math.Log(bytes, 1024); // Determine magnitude
        decimal adjustedSize = (decimal)bytes / (1L << (mag * 10));

        // Ensure rounding does not push value to next unit
        if (Math.Round(adjustedSize, decimalPlaces) >= 1024 && mag < Suffixes.Length - 1)
        {
            mag++;
            adjustedSize /= 1024;
        }

        return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, Suffixes[mag]);
    }
}