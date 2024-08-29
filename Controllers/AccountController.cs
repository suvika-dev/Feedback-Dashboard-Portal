using FDP.Data;
using FDP.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    [HttpGet]
    public IActionResult Logout()
    {
        // Sign out the user and clear authentication cookies
        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Redirect to the login page or any other page
        return RedirectToAction("Login");
    }
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user != null && VerifyPassword(user.PasswordHash, model.Password))
            {
                var role = user.UserRoles.Select(ur => ur.Role.RoleName).FirstOrDefault();

                if (role == "HR Staff" || role == "HR Manager" || role == "Admin")
                {
                    // Sign the user in
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, role)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "Login");

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("FeedbackList", "Feedback");
                }
                else
                {
                    ModelState.AddModelError("", "Access denied.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid login attempt.");
            }
        }

        return View(model);
    }

    private bool VerifyPassword(string storedHash, string enteredPassword)
    {
        // Implement your password verification logic here, for example:
        // return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
        return storedHash == enteredPassword;
    }
}

//    [HttpPost]
//    public async Task<IActionResult> Logout()
//    {
//        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
//        return RedirectToAction("Login", "Account");
//    }
//}
