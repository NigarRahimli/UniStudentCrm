using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Repositories.Coordinators;
using StudentCrm.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Repositories.Coordinators
{
    public class CoordinatorWriteRepository : WriteRepository<CoordinatorUser>, ICoordinatorWriteRepository
    {
        public CoordinatorWriteRepository(StudentCrmDbContext context) : base(context)
        {
        }
    }
}
