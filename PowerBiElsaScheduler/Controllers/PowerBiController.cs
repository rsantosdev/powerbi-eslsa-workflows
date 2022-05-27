using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PowerBiElsaScheduler.Models;
using PowerBiElsaScheduler.Services;

namespace PowerBiElsaScheduler.Controllers
{
    public class PowerBiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Export(
            [FromForm]PowerBiExportViewModel viewModel,
            [FromServices]PowerBIService pbiService)
        {
            var reportParameters = JsonSerializer.Deserialize<Dictionary<string, string?>>(viewModel.ReportParameters);
            var (excelFile, errorMessage) = await pbiService.ExportToExcelAsync(viewModel.ReportId, reportParameters);
            if (excelFile == null)
            {
                ViewData["ErrorMessage"] = errorMessage;
                return View(nameof(PowerBiController.Index), viewModel);
            }

            return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        [HttpPost("/powerbi/export-api")]
        public async Task<IActionResult> ExportApi(
            [FromBody] PowerBiExportApiViewModel viewModel,
            [FromServices] PowerBIService pbiService,
            [FromServices]ILogger<PowerBiController> logger)
        {
            var (excelFile, errorMessage) = await pbiService.ExportToExcelAsync(viewModel.ReportId, viewModel.ReportParameters);
            if (excelFile == null)
            {
                logger.LogWarning(errorMessage);
                return BadRequest(errorMessage);
            }

            return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
    }
}
