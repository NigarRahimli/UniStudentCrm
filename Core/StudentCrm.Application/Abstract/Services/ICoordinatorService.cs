using StudentCrm.Application.DTOs.Coordinator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.Abstract.Services
{
    public interface ICoordinatorService
    {
        Task<List<CoordinatorDto>> GetAllAsync();
        Task<CoordinatorDto> GetByIdAsync(string id);
        Task CreateAsync(CreateCoordinatorDto dto);
        Task UpdateAsync(UpdateCoordinatorDto dto);
        Task DeleteAsync(string id);
    }
}
