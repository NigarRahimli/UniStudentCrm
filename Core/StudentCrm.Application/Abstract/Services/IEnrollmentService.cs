using StudentCrm.Application.DTOs.Enrollment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.Abstract.Services
{
    public interface IEnrollmentService
    {
        Task<List<EnrollmentDto>> GetAllAsync();
        Task<EnrollmentDto> GetByIdAsync(string id);
        Task CreateAsync(CreateEnrollmentDto dto);
        Task UpdateAsync(UpdateEnrollmentDto dto);
        Task DeleteAsync(string id);
    }
}
