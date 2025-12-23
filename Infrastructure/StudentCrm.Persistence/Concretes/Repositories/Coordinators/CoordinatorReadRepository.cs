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
    public class CoordinatorReadRepository : ReadRepository<CoordinatorUser>, ICoordinatorReadRepository
    {
        public CoordinatorReadRepository(StudentCrmDbContext context) : base(context)
        {
        }
    }
}
