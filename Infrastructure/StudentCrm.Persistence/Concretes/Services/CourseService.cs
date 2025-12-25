using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Repositories.Courses;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Course;
using StudentCrm.Application.DTOs.Section;
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
        private readonly ISectionService _sectionService;
        private readonly IMapper _mapper;

        public CourseService(
            ICourseReadRepository courseReadRepo,
            ICourseWriteRepository courseWriteRepo,
            ISectionService sectionService,
            IMapper mapper)
        {
            _courseReadRepo = courseReadRepo;
            _courseWriteRepo = courseWriteRepo;
            _sectionService = sectionService;
            _mapper = mapper;
        }

        public async Task<List<CourseDto>> GetAllAsync()
        {
            var courses = await _courseReadRepo.GetAllAsync(
                c => !EF.Property<bool>(c, "IsDeleted"),
                include: q => q
                    .Include(c => c.Sections)
                        .ThenInclude(s => s.Term)
                    .Include(c => c.Sections)
                        .ThenInclude(s => s.Teacher),
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
                include: q => q
                    .Include(c => c.Sections)
                        .ThenInclude(s => s.Term)
                    .Include(c => c.Sections)
                        .ThenInclude(s => s.Teacher),
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

            // yalnız aktivlər arasında yoxla (filtered unique index ilə uyğun)
            var existsActive = await _courseReadRepo.GetAsync(
                c => c.Code == code && !EF.Property<bool>(c, "IsDeleted"),
                enableTracking: false
            );

            if (existsActive != null)
                throw new GlobalAppException("Bu Code ilə course artıq mövcuddur!");

            var course = new Course
            {
                Code = code,
                Title = dto.Title.Trim(),
                Credit = dto.Credit
            };

            await _courseWriteRepo.AddAsync(course);

            try
            {
                await _courseWriteRepo.CommitAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException != null &&
                                              ex.InnerException.Message.Contains("IX_Courses_Code"))
            {
                // race condition / DB unique constraint
                throw new GlobalAppException("Bu Code ilə course artıq mövcuddur!");
            }
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
                    var anotherActive = await _courseReadRepo.GetAsync(
                        c => c.Code == newCode && c.Id != courseId && !EF.Property<bool>(c, "IsDeleted"),
                        enableTracking: false
                    );

                    if (anotherActive != null)
                        throw new GlobalAppException("Bu Code ilə başqa course artıq mövcuddur!");

                    course.Code = newCode;
                }
            }

            // Title
            if (!string.IsNullOrWhiteSpace(dto.Title))
                course.Title = dto.Title.Trim();

            // Credit
            if (dto.Credit.HasValue)
                course.Credit = dto.Credit.Value;

            // ✅ Sections linking via SectionService ONLY
            // NOTE: Unlink yoxdur, çünki CourseId nullable deyil və Guid.Empty FK error yaradır.
            if (dto.SectionIds != null)
            {
                var sectionIds = dto.SectionIds
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();

                foreach (var secId in sectionIds)
                {
                    // validate + link
                    await _sectionService.GetByIdAsync(secId);

                    await _sectionService.UpdateAsync(new UpdateSectionDto
                    {
                        Id = secId,
                        CourseId = course.Id.ToString()
                    });
                }
            }

            await _courseWriteRepo.UpdateAsync(course);

            try
            {
                await _courseWriteRepo.CommitAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException != null &&
                                              ex.InnerException.Message.Contains("IX_Courses_Code"))
            {
                throw new GlobalAppException("Bu Code ilə course artıq mövcuddur!");
            }
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

            await _courseWriteRepo.SoftDeleteAsync(course);
            await _courseWriteRepo.CommitAsync();
        }
    }
}
