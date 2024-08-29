using FDP.Models;

namespace FDP.Services
{
    public interface IUserService
    {
        Task<List<UsersWithRolesViewModel>> GetUsersWithRolesAsync();
    }
}
