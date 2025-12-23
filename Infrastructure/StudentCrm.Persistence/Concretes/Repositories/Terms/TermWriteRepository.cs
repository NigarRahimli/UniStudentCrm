using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Repositories.Terms;
using StudentCrm.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Repositories.Terms
{
    public class TermWriteRepository : WriteRepository<Term>, ITermWriteRepository
    {
        public TermWriteRepository(StudentCrmDbContext context) : base(context)
        {
        }
    }
}
