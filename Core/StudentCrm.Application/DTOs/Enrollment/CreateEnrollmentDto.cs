using Student.Domain.Entities;
using StudentCrm.Application.DTOs.Section;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Enrollment
{
    public class CreateEnrollmentDto
    {
        public string StudentId { get; set; }
        public string SectionId { get; set; }
        public decimal? TotalGrade { get; set; }  // 0-100
        public string? LetterGrade { get; set; }  // A,B,C...
    }
}
