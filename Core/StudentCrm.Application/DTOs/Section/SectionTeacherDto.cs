using StudentCrm.Application.DTOs.Course;
using StudentCrm.Application.DTOs.Term;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Section
{
    public class SectionTeacherDto
    {
        public string Id { get; set; }
        public string SectionCode { get; set; }// A, B, 01...

        public CourseDto Course { get; set; }

        public TermDto Term { get; set; }
    }
}
