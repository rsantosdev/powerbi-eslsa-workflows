namespace PowerBiElsaScheduler.Models;

public class PowerBiExportApiViewModel
{
    public Guid ReportId { get; set; }
    public Dictionary<string, string> ReportParameters { get; set; }
}