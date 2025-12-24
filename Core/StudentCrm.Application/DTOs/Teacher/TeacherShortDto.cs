using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Teacher
{
    public class TeacherShortDto
    {
        public string Id { get; set; }
        public int StaffNo { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }

    }
}
