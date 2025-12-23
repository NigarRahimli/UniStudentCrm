using StudentCrm.Application.DTOs.Section;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Course
{
    public class CreateCourseDto
    {
        public string Code { get; set; } = null!;

        public string Title { get; set; } = null!;
        public int Credit { get; set; }

    }
}
