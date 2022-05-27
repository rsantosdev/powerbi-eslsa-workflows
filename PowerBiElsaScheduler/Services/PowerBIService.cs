using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using PowerBiElsaScheduler.Models;

namespace PowerBiElsaScheduler.Services;

public class PowerBIService
{
    private readonly AzureAdService _azureAdService;
    private readonly IOptions<PowerBiOptions> _pbiOptions;

    public PowerBIService(AzureAdService azureAdService, IOptions<PowerBiOptions> pbiOptions)
    {
        _azureAdService = azureAdService;
        _pbiOptions = pbiOptions;
    }

    public async Task<PowerBIClient> GetClientAsync()
    {
        var tokenCredentials = new TokenCredentials(await _azureAdService.GetAccessToken(), "Bearer");
        return new PowerBIClient(new Uri("https://api.powerbi.com"), tokenCredentials);
    }

    public async Task<(Stream?, string?)> ExportToExcelAsync(Guid reportId, Dictionary<string, string> reportParameters)
    {
        // https://docs.microsoft.com/en-us/power-bi/developer/embedded/export-paginated-report
        var exportConfiguration = new PaginatedReportExportConfiguration
        {
            ParameterValues = reportParameters
                .Select(x => new ParameterValue(x.Key, x.Value))
                .ToList()
        };

        var exportRequest = new ExportReportRequest
        {
            Format = FileFormat.XLSX,
            PaginatedReportConfiguration = exportConfiguration
        };
        
        var pbiClient = await GetClientAsync();
        var exportResponse = await pbiClient.Reports.ExportToFileInGroupWithHttpMessagesAsync(_pbiOptions.Value.WorkspaceId, 
            reportId, exportRequest);
        var exportId = exportResponse.Body.Id;
        

        // Poll Export until is ready
        Export? export = null;
        HttpOperationResponse<Export> httpMessage;
        do
        {
            httpMessage = await pbiClient.Reports.GetExportToFileStatusInGroupWithHttpMessagesAsync(_pbiOptions.Value.WorkspaceId,
                reportId, exportId);
            export = httpMessage.Body;
            if (export.Status == ExportState.Running || export.Status == ExportState.NotStarted)
            {
                // Oficial recomendation
                //var retryAfter = httpMessage.Response.Headers.RetryAfter;
                //var retryAfterInSec = retryAfter.Delta.Value.Seconds;

                // Test purporses speed up
                var retryAfterInSec = 5;

                await Task.Delay(retryAfterInSec * 1000);
            }
        } while (export.Status != ExportState.Succeeded && export.Status != ExportState.Failed);

        // Gets the file
        if (export.Status != ExportState.Succeeded)
        {
            var pbiError = await httpMessage.Response.Content.ReadFromJsonAsync<PowerBiExportError>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return (null, pbiError.Error.Message);
        }

        var fileExportResponse = await pbiClient.Reports.GetFileOfExportToFileInGroupWithHttpMessagesAsync(_pbiOptions.Value.WorkspaceId, 
            reportId, exportId);
        return (fileExportResponse.Body, null);
    }
}