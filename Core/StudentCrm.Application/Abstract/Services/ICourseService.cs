using StudentCrm.Application.DTOs.Course;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.Abstract.Services
{
    public interface ICourseService
    {
        Task<List<CourseDto>> GetAllAsync();
        Task<CourseDto> GetByIdAsync(string id);
        Task CreateAsync(CreateCourseDto dto);
        Task UpdateAsync(UpdateCourseDto dto);
        Task DeleteAsync(string id);
    }
}
