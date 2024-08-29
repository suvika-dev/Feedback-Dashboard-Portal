// Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FDP.Data;
using FDP.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using FDP.Services;

namespace FDP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public UsersController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // Action to display users with roles
        //public async Task<IActionResult> Index()
        //{
        //    var usersWithRoles = await GetUsersWithRolesAsync();
        //    return View(usersWithRoles);
        //}

        //// Method to fetch users with roles
        //private async Task<List<UsersWithRolesViewModel>> GetUsersWithRolesAsync()
        //{
        //    return await _dbContext.Users
        //        .Select(u => new UsersWithRolesViewModel
        //        {
        //            UserID = u.UserID,
        //            Username = u.Username,
        //            Roles = _dbContext.UserRoles
        //                .Where(ur => ur.UserID == u.UserID)
        //                .Select(ur => ur.Role.RoleName)
        //                .ToList()
        //        })
        //        .ToListAsync();
        //}


    }
}
