using Microsoft.AspNetCore.Identity;

namespace FDP.Models
{
    public class UserRole
    {
        public int UserRoleID { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public User User { get; set; }
        public Role Role { get; set; }
    }
}
