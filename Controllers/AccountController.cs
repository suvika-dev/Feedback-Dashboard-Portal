using FDP.Models;
using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    [HttpGet]
    public ActionResult Login()
    {
        return View();
    }
    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Perform login logic here
        }

        return View(model);
    }




    public ActionResult ForgotPassword()
    {
        // Implement forgot password logic here
        return View();
    }
}
