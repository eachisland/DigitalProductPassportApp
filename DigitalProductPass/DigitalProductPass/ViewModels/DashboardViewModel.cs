namespace DigitalProductPass.ViewModels;

public class DashboardViewModel
{
    public int Brands { get; set; }
    public int Products { get; set; }
    public int Items { get; set; }
    public int TotalScans { get; set; }
    public int ScansToday { get; set; }

    public List<string> DayLabels { get; set; } = new();
    public List<int> DayCounts { get; set; } = new();

    public List<TopProductRow> TopProducts { get; set; } = new();
    public List<RecentScanRow> RecentScans { get; set; } = new();
}

public record TopProductRow(string Name, int Scans);
public record RecentScanRow(DateTime ScannedAt, string ProductName, string SerialCode);
