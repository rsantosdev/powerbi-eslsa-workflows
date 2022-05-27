using Microsoft.AspNetCore.Mvc;

namespace PowerBiElsaScheduler.Controllers
{
    public class ElsaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
