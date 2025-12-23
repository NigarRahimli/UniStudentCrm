using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Enrollment
{
    public class UpdateEnrollmentDto
    {
        public string Id { get; set; }
        public string StudentId { get; set; }
        public string SectionId { get; set; }
        public decimal? TotalGrade { get; set; }  // 0-100
        public string? LetterGrade { get; set; }
    }
}
