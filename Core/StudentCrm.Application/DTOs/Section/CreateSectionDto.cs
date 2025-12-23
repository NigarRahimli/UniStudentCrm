using Student.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Section
{
    public class CreateSectionDto
    {
        public string SectionCode { get; set; } // A, B, 01...

        public string CourseId { get; set; }

        public string TermId { get; set; }

        public string TeacherId { get; set; }

    }
}
