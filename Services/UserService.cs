// UserService.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FDP.Data;
using FDP.Models;
using FDP.Services;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;

    public UserService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<UsersWithRolesViewModel>> GetUsersWithRolesAsync()
    {
        return await _dbContext.Users
            .Select(u => new UsersWithRolesViewModel
            {
                UserID = u.UserID,
                Username = u.Username,
                Roles = _dbContext.UserRoles
                    .Where(ur => ur.UserID == u.UserID)
                    .Select(ur => ur.Role.RoleName)
                    .ToList()
            })
            .ToListAsync();
    }
}
