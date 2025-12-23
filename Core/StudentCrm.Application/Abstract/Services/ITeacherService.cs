using StudentCrm.Application.DTOs.Teacher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.Abstract.Services
{
    public interface ITeacherService
    {
        Task<List<TeacherDto>> GetAllAsync();
        Task<TeacherDto> GetByIdAsync(string id);
        Task CreateAsync(CreateTeacherDto dto);
        Task UpdateAsync(UpdateTeacherDto dto);
        Task DeleteAsync(string id);
    }
}
