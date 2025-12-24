using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Coordinator
{
    public class CreateCoordinatorDto
    {
        public string FullName { get; set; } = null!;
        public string? Department { get; set; }
        public int CoordinatorNo { get; set; }

        // login üçün (AppUser yaratmaq üçün)
        public string Email { get; set; } = null!;
       
    }
}
