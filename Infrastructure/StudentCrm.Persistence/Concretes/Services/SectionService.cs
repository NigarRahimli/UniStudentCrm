using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Repositories.Courses;
using StudentCrm.Application.Abstract.Repositories.Sections;
using StudentCrm.Application.Abstract.Repositories.Teachers;
using StudentCrm.Application.Abstract.Repositories.Terms;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Section;
using StudentCrm.Application.GlobalAppException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class SectionService : ISectionService
    {
        private readonly ISectionReadRepository _sectionReadRepo;
        private readonly ISectionWriteRepository _sectionWriteRepo;
        private readonly ICourseReadRepository _courseReadRepo;
        private readonly ITermReadRepository _termReadRepo;
        private readonly ITeacherReadRepository _teacherReadRepo;
        private readonly IMapper _mapper;

        public SectionService(
            ISectionReadRepository sectionReadRepo,
            ISectionWriteRepository sectionWriteRepo,
            ICourseReadRepository courseReadRepo,
            ITermReadRepository termReadRepo,
            ITeacherReadRepository teacherReadRepo,
            IMapper mapper)
        {
            _sectionReadRepo = sectionReadRepo;
            _sectionWriteRepo = sectionWriteRepo;
            _courseReadRepo = courseReadRepo;
            _termReadRepo = termReadRepo;
            _teacherReadRepo = teacherReadRepo;
            _mapper = mapper;
        }

        public async Task<List<SectionDto>> GetAllAsync()
        {
            var sections = await _sectionReadRepo.GetAllAsync(
                s => !EF.Property<bool>(s, "IsDeleted"),
                include: q => q
                    .Include(s => s.Course)
                    .Include(s => s.Term)
                    .Include(s => s.Teacher)
                    .Include(s => s.Enrollments)
                        .ThenInclude(e => e.Student)
                        .ThenInclude(s => s.AppUser),
                enableTracking: false
            );

            return _mapper.Map<List<SectionDto>>(sections.ToList());
        }

        public async Task<SectionDto> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var sectionId))
                throw new GlobalAppException("Invalid Section ID!");

            var section = await _sectionReadRepo.GetAsync(
                s => s.Id == sectionId && !EF.Property<bool>(s, "IsDeleted"),
                include: q => q
                    .Include(s => s.Course)
                    .Include(s => s.Term)
                    .Include(s => s.Teacher)
                    .Include(s => s.Enrollments)
                        .ThenInclude(e => e.Student),
                enableTracking: false
            );

            if (section == null)
                throw new GlobalAppException("Section not found!");

            return _mapper.Map<SectionDto>(section);
        }

        public async Task CreateAsync(CreateSectionDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Submitted data is null!");

            if (string.IsNullOrWhiteSpace(dto.SectionCode))
                throw new GlobalAppException("SectionCode cannot be empty!");

            if (string.IsNullOrWhiteSpace(dto.CourseId) || !Guid.TryParse(dto.CourseId, out var courseId))
                throw new GlobalAppException("Invalid Course ID!");

            if (string.IsNullOrWhiteSpace(dto.TermId) || !Guid.TryParse(dto.TermId, out var termId))
                throw new GlobalAppException("Invalid Term ID!");

            if (string.IsNullOrWhiteSpace(dto.TeacherId) || !Guid.TryParse(dto.TeacherId, out var teacherId))
                throw new GlobalAppException("Invalid Teacher ID!");

            // Check related exists (and not deleted)
            var course = await _courseReadRepo.GetAsync(c => c.Id == courseId && !EF.Property<bool>(c, "IsDeleted"));
            if (course == null)
                throw new GlobalAppException("Course not found!");

            var term = await _termReadRepo.GetAsync(t => t.Id == termId && !EF.Property<bool>(t, "IsDeleted"));
            if (term == null)
                throw new GlobalAppException("Term not found!");

            var teacher = await _teacherReadRepo.GetAsync(t => t.Id == teacherId && !EF.Property<bool>(t, "IsDeleted"));
            if (teacher == null)
                throw new GlobalAppException("Teacher not found!");

            var code = dto.SectionCode.Trim();

            // Prevent duplicate SectionCode in same course & term
            var dup = await _sectionReadRepo.GetAsync(s =>
                s.SectionCode == code &&
                s.CourseId == courseId &&
                s.TermId == termId &&
                !EF.Property<bool>(s, "IsDeleted"));

            if (dup != null)
                throw new GlobalAppException("This section code already exists for this course and term!");

            var section = new Section
            {
                SectionCode = code,
                CourseId = courseId,
                TermId = termId,
                TeacherId = teacherId
            };

            await _sectionWriteRepo.AddAsync(section);
            await _sectionWriteRepo.CommitAsync();
        }

        public async Task UpdateAsync(UpdateSectionDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Invalid Section data!");

            if (string.IsNullOrWhiteSpace(dto.Id) || !Guid.TryParse(dto.Id, out var sectionId))
                throw new GlobalAppException("Invalid Section ID!");

            var section = await _sectionReadRepo.GetAsync(
                s => s.Id == sectionId && !EF.Property<bool>(s, "IsDeleted"),
                enableTracking: true
            );

            if (section == null)
                throw new GlobalAppException("Section not found!");

            // keep old values (optional, but matches your partial update style)
            var oldCourseId = section.CourseId;
            var oldTermId = section.TermId;
            var oldTeacherId = section.TeacherId;
            var oldCode = section.SectionCode;

            // SectionCode (optional)
            if (dto.SectionCode != null)
                section.SectionCode = string.IsNullOrWhiteSpace(dto.SectionCode) ? oldCode : dto.SectionCode.Trim();
            else
                section.SectionCode = oldCode;

            // Course (optional)
            if (dto.CourseId != null)
            {
                if (string.IsNullOrWhiteSpace(dto.CourseId) || !Guid.TryParse(dto.CourseId, out var courseId))
                    throw new GlobalAppException("Invalid Course ID!");

                var course = await _courseReadRepo.GetAsync(c => c.Id == courseId && !EF.Property<bool>(c, "IsDeleted"));
                if (course == null)
                    throw new GlobalAppException("Course not found!");

                section.CourseId = courseId;
            }
            else
            {
                section.CourseId = oldCourseId;
            }

            // Term (optional)
            if (dto.TermId != null)
            {
                if (string.IsNullOrWhiteSpace(dto.TermId) || !Guid.TryParse(dto.TermId, out var termId))
                    throw new GlobalAppException("Invalid Term ID!");

                var term = await _termReadRepo.GetAsync(t => t.Id == termId && !EF.Property<bool>(t, "IsDeleted"));
                if (term == null)
                    throw new GlobalAppException("Term not found!");

                section.TermId = termId;
            }
            else
            {
                section.TermId = oldTermId;
            }

            // Teacher (optional)
            if (dto.TeacherId != null)
            {
                if (string.IsNullOrWhiteSpace(dto.TeacherId) || !Guid.TryParse(dto.TeacherId, out var teacherId))
                    throw new GlobalAppException("Invalid Teacher ID!");

                var teacher = await _teacherReadRepo.GetAsync(t => t.Id == teacherId && !EF.Property<bool>(t, "IsDeleted"));
                if (teacher == null)
                    throw new GlobalAppException("Teacher not found!");

                section.TeacherId = teacherId;
            }
            else
            {
                section.TeacherId = oldTeacherId;
            }

            // If SectionCode/CourseId/TermId changed, re-check duplicate rule
            var dup = await _sectionReadRepo.GetAsync(s =>
                s.Id != sectionId &&
                s.SectionCode == section.SectionCode &&
                s.CourseId == section.CourseId &&
                s.TermId == section.TermId &&
                !EF.Property<bool>(s, "IsDeleted"));

            if (dup != null)
                throw new GlobalAppException("This section code already exists for this course and term!");

            await _sectionWriteRepo.UpdateAsync(section);
            await _sectionWriteRepo.CommitAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var sectionId))
                throw new GlobalAppException("Invalid Section ID!");

            var section = await _sectionReadRepo.GetAsync(
                s => s.Id == sectionId && !EF.Property<bool>(s, "IsDeleted"),
                enableTracking: true
            );

            if (section == null)
                throw new GlobalAppException("Section not found!");

            // Choose ONE delete style:

            // Hard delete:
            // await _sectionWriteRepo.HardDeleteAsync(section);

            // Soft delete (recommended):
            await _sectionWriteRepo.SoftDeleteAsync(section);

            await _sectionWriteRepo.CommitAsync();
        }
    }
}
