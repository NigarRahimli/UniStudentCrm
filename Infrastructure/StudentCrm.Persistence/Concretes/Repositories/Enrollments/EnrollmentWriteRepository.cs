using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Repositories.Enrollments;
using StudentCrm.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Repositories.Enrollments
{
    public class EnrollmentWriteRepository : WriteRepository<Enrollment>, IEnrollmentWriteRepository
    {
        public EnrollmentWriteRepository(StudentCrmDbContext context) : base(context)
        {
        }
    }
}
