using Student.Domain.Entities;
using StudentCrm.Application.DTOs.Course;
using StudentCrm.Application.DTOs.Enrollment;
using StudentCrm.Application.DTOs.Teacher;
using StudentCrm.Application.DTOs.Term;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Section
{
    public class SectionDto
    {
        public string Id { get; set; }
        public string SectionCode { get; set; }// A, B, 01...

        public CourseShortDto Course { get; set; }

        public TermShortDto Term { get; set; }

        public TeacherShortDto? Teacher { get; set; }

        public List<EnrollmentShortDto>? Enrollments { get; set; }
    }
}
