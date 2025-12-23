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
    public class StudentWriteRepository : WriteRepository<StudentUser>, IStudentWriteRepository
    {
        public StudentWriteRepository(StudentCrmDbContext context) : base(context)
        {
        }
    }
}
