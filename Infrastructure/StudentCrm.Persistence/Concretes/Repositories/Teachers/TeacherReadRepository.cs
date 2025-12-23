using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Repositories.Teachers;
using StudentCrm.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Repositories.Teachers
{
    public class TeacherReadRepository : ReadRepository<TeacherUser>, ITeacherReadRepository
    {
        public TeacherReadRepository(StudentCrmDbContext context) : base(context)
        {
        }
    }
}
