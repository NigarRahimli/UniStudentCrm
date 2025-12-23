using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.Abstract.Repositories.Sections
{
    public interface ISectionWriteRepository: IWriteRepository<Section>
    {
    }
}
