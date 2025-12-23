using StudentCrm.Application.DTOs.Section;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.Abstract.Services
{
    public interface ISectionService
    {
        Task<List<SectionDto>> GetAllAsync();
        Task<SectionDto> GetByIdAsync(string id);
        Task CreateAsync(CreateSectionDto dto);
        Task UpdateAsync(UpdateSectionDto dto);
        Task DeleteAsync(string id);
    }
}
