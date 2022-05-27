namespace PowerBiElsaScheduler.Models
{
    public class AzureAdOptions
    {
        public string AuthorityUri { get; set; }

        public string ClientId { get; set; }

        public string TenantId { get; set; }

        public string[] Scope { get; set; }

        public string ClientSecret { get; set; }
    }
}
