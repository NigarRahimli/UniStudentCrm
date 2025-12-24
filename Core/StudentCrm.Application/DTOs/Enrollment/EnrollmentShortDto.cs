using StudentCrm.Application.DTOs.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Enrollment
{
    public class EnrollmentShortDto
    {
        public decimal? TotalGrade { get; set; }  // 0-100
        public string? LetterGrade { get; set; }
        public StudentDto Student { get; set; } = null!;
    }
}
