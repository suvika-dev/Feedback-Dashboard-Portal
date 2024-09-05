using FDP.Data; // Adjust to your actual namespace
using FDP.Models;
using FDP.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using FDP.Middleware;


var builder = WebApplication.CreateBuilder(args);



// Configure Entity Framework Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie( options =>
    {
        options.LoginPath = "/Account/Login";        
        options.LogoutPath = "/Account/Logout";
       options.AccessDeniedPath = "/Account/AccessDenied";
       // options.SlidingExpiration = true;// This refreshes the expiration time with every request
    });

// Add services to the container.
builder.Services.AddControllersWithViews();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
// Use custom middleware to clear cookies
//app.UseMiddleware<ClearCookiesMiddleware>();

app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
