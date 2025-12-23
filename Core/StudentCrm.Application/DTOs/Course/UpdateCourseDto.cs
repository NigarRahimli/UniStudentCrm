using StudentCrm.Application.DTOs.Section;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Course
{
    public class UpdateCourseDto
    {
        public string Id { get; set; }
        public string? Code { get; set; }

        public string? Title { get; set; } = null!;
        public int? Credit { get; set; }

        public List<string>? SectionIds { get; set; }
    }
}
