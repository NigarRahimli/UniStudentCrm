using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Course;
using StudentCrm.Application.GlobalAppException;
using Student.Domain.Entities;
using StudentCrm.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class CourseService : ICourseService
    {
        private readonly StudentCrmDbContext _db;
        private readonly IMapper _mapper;

        public CourseService(StudentCrmDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<CourseDto>> GetAllAsync()
        {
            var courses = await _db.Courses
                .Include(c => c.Sections)
                .ToListAsync();

            return _mapper.Map<List<CourseDto>>(courses);
        }

        public async Task<CourseDto> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var courseId))
                throw new GlobalAppException("Invalid Course ID!");

            var course = await _db.Courses
                .Include(c => c.Sections)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                throw new GlobalAppException("Course not found!");

            return _mapper.Map<CourseDto>(course);
        }

        public async Task CreateAsync(CreateCourseDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Submitted data is null!");

            if (string.IsNullOrWhiteSpace(dto.Code))
                throw new GlobalAppException("Code cannot be empty!");

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new GlobalAppException("Title cannot be empty!");

            var code = dto.Code.Trim();

            if (await _db.Courses.AnyAsync(c => c.Code == code))
                throw new GlobalAppException("A course with this code already exists!");

            var course = new Course
            {
                Code = code,
                Title = dto.Title.Trim(),
                Credit = dto.Credit
            };

            _db.Courses.Add(course);
            await _db.SaveChangesAsync();

        }

        public async Task UpdateAsync(UpdateCourseDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Invalid Course update data!");

            if (string.IsNullOrWhiteSpace(dto.Id) || !Guid.TryParse(dto.Id, out var courseId))
                throw new GlobalAppException("Invalid Course ID!");

            var course = await _db.Courses
                .Include(c => c.Sections)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                throw new GlobalAppException("Course not found!");

            // Code
            if (!string.IsNullOrWhiteSpace(dto.Code))
            {
                var newCode = dto.Code.Trim();

                if (!string.Equals(course.Code, newCode, StringComparison.OrdinalIgnoreCase) &&
                    await _db.Courses.AnyAsync(c => c.Code == newCode && c.Id != courseId))
                {
                    throw new GlobalAppException("Another course with this code already exists!");
                }

                course.Code = newCode;
            }

            // Title
            if (!string.IsNullOrWhiteSpace(dto.Title))
                course.Title = dto.Title.Trim();

            // Credit
            if (dto.Credit.HasValue)
                course.Credit = dto.Credit.Value;

            // Sections linking (dto.SectionIds is List<string>?)
            if (dto.SectionIds != null)
            {
                // 1) Unlink sections not in dto list
                foreach (var sec in course.Sections.ToList())
                {
                    if (!dto.SectionIds.Contains(sec.Id.ToString()))
                        sec.CourseId = Guid.Empty;
                }

                // 2) Link new sections
                foreach (var secIdStr in dto.SectionIds)
                {
                    if (!Guid.TryParse(secIdStr, out var secId))
                        throw new GlobalAppException($"Invalid Section ID: {secIdStr}");

                    if (!course.Sections.Any(s => s.Id == secId))
                    {
                        var section = await _db.Sections.FindAsync(secId);
                        if (section == null)
                            throw new GlobalAppException($"Section with ID {secIdStr} not found!");

                        section.CourseId = course.Id;
                    }
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var courseId))
                throw new GlobalAppException("Invalid Course ID!");

            var course = await _db.Courses.FindAsync(courseId);
            if (course == null)
                throw new GlobalAppException("Course not found!");

            // unlink sections
            var sections = await _db.Sections
                .Where(s => s.CourseId == courseId)
                .ToListAsync();

            foreach (var sec in sections)
                sec.CourseId = Guid.Empty;

            _db.Courses.Remove(course);
            await _db.SaveChangesAsync();
        }
    }
}
