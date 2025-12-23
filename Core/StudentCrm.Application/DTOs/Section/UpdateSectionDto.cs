using Student.Domain.Entities;
using StudentCrm.Application.DTOs.Enrollment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Section
{
    public class UpdateSectionDto
    {
        public string Id { get; set; }
        public string SectionCode { get; set; } = null!; // A, B, 01...

        public string? CourseId { get; set; }

        public string? TermId { get; set; }

        public string? TeacherId { get; set; }
    }
}
