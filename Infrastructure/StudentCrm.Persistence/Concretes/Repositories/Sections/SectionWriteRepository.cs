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
    public class SectionWriteRepository : WriteRepository<Section>, ISectionWriteRepository
    {
        public SectionWriteRepository(StudentCrmDbContext context) : base(context)
        {
        }
    }
}
