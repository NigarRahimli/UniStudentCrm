
using Microsoft.AspNetCore.Identity;

namespace Student.Domain.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
      

        // Temporary password flow (sənin istədiyin məntiq)
        public bool MustChangePassword { get; set; } = true;
        public DateTime? TemporaryPasswordIssuedAt { get; set; }


        //public string Phone { get; set; }
        // public string Email { get; set; }
    }
}
