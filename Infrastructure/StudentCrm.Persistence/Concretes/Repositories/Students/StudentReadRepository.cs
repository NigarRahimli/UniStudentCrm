using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Repositories.Students;
using StudentCrm.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Repositories.Students
{
    public class StudentReadRepository : ReadRepository<StudentUser>, IStudentReadRepository
    {
        public StudentReadRepository(StudentCrmDbContext context) : base(context)
        {
        }
    }
}
