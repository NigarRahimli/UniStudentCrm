
using Microsoft.AspNetCore.Identity;

namespace Student.Domain.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        //public string Phone { get; set; }
       // public string Email { get; set; }
    }
}
