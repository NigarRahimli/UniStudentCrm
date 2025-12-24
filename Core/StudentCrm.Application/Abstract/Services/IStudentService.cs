using StudentCrm.Application.DTOs.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.Abstract.Services
{
    public interface IStudentService
    {
        Task<List<StudentDto>> GetAllAsync();
        Task<StudentDetailDto> GetByIdAsync(string id);
        Task CreateAsync(CreateStudentDto dto);
        Task UpdateAsync(UpdateStudentDto dto);
        Task DeleteAsync(string id);

    }
}
