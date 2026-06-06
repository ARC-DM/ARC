namespace ArcDaemon.Services;

public static class ProcessHelper
{
    public static string GetTopProcessesFormatted(int count = 5)
    {
        var processes = System.Diagnostics.Process.GetProcesses()
            .Select(p => {
                try {
                    long ramMb = p.WorkingSet64 / 1024 / 1024;
                    return new { p.ProcessName, p.Id, ramMb };
                }
                catch { return null; }
                finally { p.Dispose(); }
            })
            .Where(p => p != null)
            .OrderByDescending(p => p!.ramMb)
            .Take(count);

        return string.Join("\n", processes.Select(p => 
            $"{p!.ProcessName,-20} PID: {p.Id,-8} RAM: {p.ramMb} MB"));
    }
}