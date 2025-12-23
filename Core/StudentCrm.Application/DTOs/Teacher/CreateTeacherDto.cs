using Student.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Teacher
{
    public class CreateTeacherDto
    {
        public int StaffNo { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }

        public List<string> SectionIds { get; set; } 
    }
}
