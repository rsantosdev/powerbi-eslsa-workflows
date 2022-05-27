using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using PowerBiElsaScheduler.Models;

namespace PowerBiElsaScheduler.Services
{
    public class AzureAdService
    {
        private readonly IOptions<AzureAdOptions> _azureAd;

        public AzureAdService(IOptions<AzureAdOptions> azureAd)
        {
            _azureAd = azureAd;
        }

        /// <summary>
        /// Generates and returns Access token
        /// </summary>
        /// <returns>AAD token</returns>
        public async Task<string> GetAccessToken()
        {
            // For app only authentication, we need the specific tenant id in the authority url
            var tenantSpecificUrl = _azureAd.Value.AuthorityUri.Replace("organizations", _azureAd.Value.TenantId);

            // Create a confidential client to authorize the app with the AAD app
            var clientApp = ConfidentialClientApplicationBuilder
                .Create(_azureAd.Value.ClientId)
                .WithClientSecret(_azureAd.Value.ClientSecret)
                .WithAuthority(tenantSpecificUrl)
                .Build();

            // TODO: store this in redis
            var authenticationResult = await clientApp.AcquireTokenForClient(_azureAd.Value.Scope)
                .ExecuteAsync();
            
            return authenticationResult.AccessToken;
        }
    }
}
