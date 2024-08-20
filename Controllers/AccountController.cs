using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FDP.Models;
namespace FDP.Controllers
{
    public class AccountController: Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
