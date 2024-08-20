using Microsoft.AspNetCore.Mvc;

namespace FDP.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult GenerateReport()
        {
            return View();
        }
    }
}
