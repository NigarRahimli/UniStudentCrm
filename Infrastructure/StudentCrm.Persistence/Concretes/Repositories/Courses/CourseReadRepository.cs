using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Repositories.Courses;
using StudentCrm.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Repositories.Courses
{
    public class CourseReadRepository : ReadRepository<Course>, ICourseReadRepository
    {
        public CourseReadRepository(StudentCrmDbContext context) : base(context)
        {
        }
    }
}
