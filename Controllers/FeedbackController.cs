using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using   FDP.Models;
namespace FDP.Controllers
{
    public class FeedbackController: Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
