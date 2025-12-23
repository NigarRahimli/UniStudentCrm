using Student.Domain.Entities;
using StudentCrm.Application.DTOs.Section;
using StudentCrm.Application.DTOs.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Enrollment
{
    public class EnrollmentDto
    {
        public StudentDto Student { get; set; } = null!;
        public SectionDto Section { get; set; } = null!;

        public decimal? TotalGrade { get; set; }  // 0-100
        public string? LetterGrade { get; set; }  // A,B,C...
    }
}
