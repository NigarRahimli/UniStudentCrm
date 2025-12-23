using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Section;
using StudentCrm.Application.GlobalAppException;
using Student.Domain.Entities;
using StudentCrm.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class SectionService : ISectionService
    {
        private readonly StudentCrmDbContext _db;
        private readonly IMapper _mapper;

        public SectionService(StudentCrmDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<SectionDto>> GetAllAsync()
        {
            var sections = await _db.Sections
                .Include(s => s.Course)
                .Include(s => s.Term)
                .Include(s => s.Teacher)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Student)
                .ToListAsync();

            return _mapper.Map<List<SectionDto>>(sections);
        }

        public async Task<SectionDto> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var sectionId))
                throw new GlobalAppException("Invalid Section ID!");

            var section = await _db.Sections
                .Include(s => s.Course)
                .Include(s => s.Term)
                .Include(s => s.Teacher)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

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

            // Check related exists
            var course = await _db.Courses.FindAsync(courseId);
            if (course == null)
                throw new GlobalAppException("Course not found!");

            var term = await _db.Terms.FindAsync(termId);
            if (term == null)
                throw new GlobalAppException("Term not found!");

            var teacher = await _db.Teachers.FindAsync(teacherId);
            if (teacher == null)
                throw new GlobalAppException("Teacher not found!");

            // Prevent duplicate SectionCode in same course & term
            bool exists = await _db.Sections.AnyAsync(s =>
                s.SectionCode == dto.SectionCode.Trim() &&
                s.CourseId == courseId &&
                s.TermId == termId);

            if (exists)
                throw new GlobalAppException("This section code already exists for this course and term!");

            var section = new Section
            {
                SectionCode = dto.SectionCode.Trim(),
                CourseId = courseId,
                TermId = termId,
                TeacherId = teacherId
            };

            _db.Sections.Add(section);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(UpdateSectionDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Invalid Section data!");

            if (string.IsNullOrWhiteSpace(dto.Id) || !Guid.TryParse(dto.Id, out var sectionId))
                throw new GlobalAppException("Invalid Section ID!");

            var section = await _db.Sections.FindAsync(sectionId);
            if (section == null)
                throw new GlobalAppException("Section not found!");

            // SectionCode
            if (!string.IsNullOrWhiteSpace(dto.SectionCode))
                section.SectionCode = dto.SectionCode.Trim();

            // Optional update Course
            if (!string.IsNullOrWhiteSpace(dto.CourseId))
            {
                if (!Guid.TryParse(dto.CourseId, out var courseId))
                    throw new GlobalAppException("Invalid Course ID!");

                var course = await _db.Courses.FindAsync(courseId);
                if (course == null)
                    throw new GlobalAppException("Course not found!");

                section.CourseId = courseId;
            }

            // Optional update Term
            if (!string.IsNullOrWhiteSpace(dto.TermId))
            {
                if (!Guid.TryParse(dto.TermId, out var termId))
                    throw new GlobalAppException("Invalid Term ID!");

                var term = await _db.Terms.FindAsync(termId);
                if (term == null)
                    throw new GlobalAppException("Term not found!");

                section.TermId = termId;
            }

            // Optional update Teacher
            if (!string.IsNullOrWhiteSpace(dto.TeacherId))
            {
                if (!Guid.TryParse(dto.TeacherId, out var teacherId))
                    throw new GlobalAppException("Invalid Teacher ID!");

                var teacher = await _db.Teachers.FindAsync(teacherId);
                if (teacher == null)
                    throw new GlobalAppException("Teacher not found!");

                section.TeacherId = teacherId;
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var sectionId))
                throw new GlobalAppException("Invalid Section ID!");

            var section = await _db.Sections.FindAsync(sectionId);
            if (section == null)
                throw new GlobalAppException("Section not found!");

            _db.Sections.Remove(section);
            await _db.SaveChangesAsync();
        }
    }
}
