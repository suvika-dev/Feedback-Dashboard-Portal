using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FDP.Models;
namespace FDP.Controllers
{
    public class ReportsController: Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
