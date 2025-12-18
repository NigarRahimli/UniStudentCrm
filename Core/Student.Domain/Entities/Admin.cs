
using Microsoft.AspNetCore.Identity;

namespace Student.Domain.Entities
{
    public class Admin : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
    }
}
