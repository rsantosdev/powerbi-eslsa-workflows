namespace PowerBiElsaScheduler.Models
{
    public class PowerBiExportError
    {
        public PowerBiErrorDescription Error { get; set; }
    }

    public class PowerBiErrorDescription
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
