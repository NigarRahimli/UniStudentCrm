using Student.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Coordinator
{
    public class CoordinatorDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? Department { get; set; }
        public int CoordinatorNo { get; set; }

        public Guid AppUserId { get; set; }
        public string? Email { get; set; } // from AppUser.Email
    }
}
