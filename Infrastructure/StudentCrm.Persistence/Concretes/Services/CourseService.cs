using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Repositories.Courses;
using StudentCrm.Application.Abstract.Repositories.Sections;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Course;
using StudentCrm.Application.GlobalAppException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseReadRepository _courseReadRepo;
        private readonly ICourseWriteRepository _courseWriteRepo;
        private readonly ISectionReadRepository _sectionReadRepo;
        private readonly ISectionWriteRepository _sectionWriteRepo;
        private readonly IMapper _mapper;

        public CourseService(
            ICourseReadRepository courseReadRepo,
            ICourseWriteRepository courseWriteRepo,
            ISectionReadRepository sectionReadRepo,
            ISectionWriteRepository sectionWriteRepo,
            IMapper mapper)
        {
            _courseReadRepo = courseReadRepo;
            _courseWriteRepo = courseWriteRepo;
            _sectionReadRepo = sectionReadRepo;
            _sectionWriteRepo = sectionWriteRepo;
            _mapper = mapper;
        }

        public async Task<List<CourseDto>> GetAllAsync()
        {
            var courses = await _courseReadRepo.GetAllAsync(
                c => !EF.Property<bool>(c, "IsDeleted"),
                include: q => q.Include(c => c.Sections),
                enableTracking: false
            );

            return _mapper.Map<List<CourseDto>>(courses.ToList());
        }

        public async Task<CourseDto> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var courseId))
                throw new GlobalAppException("Invalid Course ID!");

            var course = await _courseReadRepo.GetAsync(
                c => c.Id == courseId && !EF.Property<bool>(c, "IsDeleted"),
                include: q => q.Include(c => c.Sections),
                enableTracking: false
            );

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

            // Unique code
            var existing = await _courseReadRepo.GetAsync(
                c => c.Code == code && !EF.Property<bool>(c, "IsDeleted"),
                enableTracking: false
            );

            if (existing != null)
                throw new GlobalAppException("A course with this code already exists!");

            var course = new Course
            {
                Code = code,
                Title = dto.Title.Trim(),
                Credit = dto.Credit
            };

            await _courseWriteRepo.AddAsync(course);
            await _courseWriteRepo.CommitAsync();
        }

        public async Task UpdateAsync(UpdateCourseDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Invalid Course update data!");

            if (string.IsNullOrWhiteSpace(dto.Id) || !Guid.TryParse(dto.Id, out var courseId))
                throw new GlobalAppException("Invalid Course ID!");

            var course = await _courseReadRepo.GetAsync(
                c => c.Id == courseId && !EF.Property<bool>(c, "IsDeleted"),
                include: q => q.Include(c => c.Sections),
                enableTracking: true
            );

            if (course == null)
                throw new GlobalAppException("Course not found!");

            // Code
            if (!string.IsNullOrWhiteSpace(dto.Code))
            {
                var newCode = dto.Code.Trim();

                if (!string.Equals(course.Code, newCode, StringComparison.OrdinalIgnoreCase))
                {
                    var another = await _courseReadRepo.GetAsync(
                        c => c.Code == newCode && c.Id != courseId && !EF.Property<bool>(c, "IsDeleted"),
                        enableTracking: false
                    );

                    if (another != null)
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
                    {
                        // better than Guid.Empty: make nullable in entity if you can
                        sec.CourseId = Guid.Empty;
                        await _sectionWriteRepo.UpdateAsync(sec);
                    }
                }

                // 2) Link new sections
                foreach (var secIdStr in dto.SectionIds)
                {
                    if (!Guid.TryParse(secIdStr, out var secId))
                        throw new GlobalAppException($"Invalid Section ID: {secIdStr}");

                    if (!course.Sections.Any(s => s.Id == secId))
                    {
                        var section = await _sectionReadRepo.GetAsync(
                            s => s.Id == secId && !EF.Property<bool>(s, "IsDeleted"),
                            enableTracking: true
                        );

                        if (section == null)
                            throw new GlobalAppException($"Section with ID {secIdStr} not found!");

                        section.CourseId = course.Id;
                        await _sectionWriteRepo.UpdateAsync(section);
                    }
                }
            }

            await _courseWriteRepo.UpdateAsync(course);
            await _courseWriteRepo.CommitAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var courseId))
                throw new GlobalAppException("Invalid Course ID!");

            var course = await _courseReadRepo.GetAsync(
                c => c.Id == courseId && !EF.Property<bool>(c, "IsDeleted"),
                enableTracking: true
            );

            if (course == null)
                throw new GlobalAppException("Course not found!");

            // unlink sections
            var sections = await _sectionReadRepo.GetAllAsync(
                s => s.CourseId == courseId && !EF.Property<bool>(s, "IsDeleted"),
                enableTracking: true
            );

            foreach (var sec in sections)
            {
                sec.CourseId = Guid.Empty;
                await _sectionWriteRepo.UpdateAsync(sec);
            }

            // choose ONE delete style:

            // Hard delete:
            // await _courseWriteRepo.HardDeleteAsync(course);

            // Soft delete (recommended if you use IsDeleted everywhere):
            await _courseWriteRepo.SoftDeleteAsync(course);

            await _courseWriteRepo.CommitAsync();
        }
    }
}
