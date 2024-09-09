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
        return RedirectToAction("Index","Home");
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
                var userRole = user.UserRoles.FirstOrDefault();
                var roleName = userRole?.Role?.RoleName;
                var roleId = userRole?.Role?.RoleID;

                
                    // Creating claims for the authenticated user
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, roleName),
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()), // Standard claim for UserID
                    new Claim("RoleID", roleId.ToString()) // Custom claim for RoleID
                };

                    // Creating the identity and signing in the user
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // Sign in the user with the claims principal
                     await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
                    
                    return RedirectToAction("LandingPage", "Home");
                    
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


