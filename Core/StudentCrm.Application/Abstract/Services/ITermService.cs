using StudentCrm.Application.DTOs.Teacher;
using StudentCrm.Application.DTOs.Term;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.Abstract.Services
{
    public interface ITermService
    {
        Task<List<TermDto>> GetAllAsync();
        Task<TermDto> GetByIdAsync(string id);
        Task CreateAsync(CreateTermDto dto);
        Task UpdateAsync(UpdateTermDto dto);
        Task DeleteAsync(string id);
    }
}
