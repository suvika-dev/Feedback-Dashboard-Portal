using Microsoft.AspNetCore.Identity;
namespace FDP.Models
{
    public class User
    {
         public int UserID { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
    }
}
