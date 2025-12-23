using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Repositories.Sections;
using StudentCrm.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Repositories.Sections
{
    public class SectionReadRepository : ReadRepository<Section>, ISectionReadRepository
    {
        public SectionReadRepository(StudentCrmDbContext context) : base(context)
        {
        }
    }
}
