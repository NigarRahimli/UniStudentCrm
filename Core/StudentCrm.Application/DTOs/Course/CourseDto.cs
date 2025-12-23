using StudentCrm.Application.DTOs.Section;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Course
{
    public class CourseDto
    {
        public string Id { get; set; }
        public string? Code { get; set; } = null!;

        public string? Title { get; set; } = null!;
        public int?  Credit { get; set; }

        public List<SectionDto> Sections { get; set; } = new List<SectionDto>();
    }
}
