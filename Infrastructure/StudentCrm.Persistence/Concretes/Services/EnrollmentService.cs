using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Enrollment;
using StudentCrm.Application.GlobalAppException;
using Student.Domain.Entities;
using StudentCrm.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly StudentCrmDbContext _db;
        private readonly IMapper _mapper;

        public EnrollmentService(StudentCrmDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<EnrollmentDto>> GetAllAsync()
        {
            var list = await _db.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Section)
                .ToListAsync();

            return _mapper.Map<List<EnrollmentDto>>(list);
        }

        public async Task<EnrollmentDto> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var enrollmentId))
                throw new GlobalAppException("Invalid Enrollment ID!");

            var entity = await _db.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Section)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (entity == null)
                throw new GlobalAppException("Enrollment not found!");

            return _mapper.Map<EnrollmentDto>(entity);
        }

        public async Task CreateAsync(CreateEnrollmentDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Submitted data is null!");

            if (string.IsNullOrWhiteSpace(dto.StudentId) || !Guid.TryParse(dto.StudentId, out var studentId))
                throw new GlobalAppException("Student must be specified!");

            if (string.IsNullOrWhiteSpace(dto.SectionId) || !Guid.TryParse(dto.SectionId, out var sectionId))
                throw new GlobalAppException("Section must be specified!");

            var student = await _db.Students.FindAsync(studentId)
                ?? throw new GlobalAppException("Student not found!");

            var section = await _db.Sections.FindAsync(sectionId)
                ?? throw new GlobalAppException("Section not found!");

            bool exists = await _db.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.SectionId == sectionId);

            if (exists)
                throw new GlobalAppException("This student is already enrolled in this section!");

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                SectionId = sectionId,
                TotalGrade = dto.TotalGrade,
                LetterGrade = dto.LetterGrade
            };

            _db.Enrollments.Add(enrollment);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(UpdateEnrollmentDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Id) || !Guid.TryParse(dto.Id, out var enrollmentId))
                throw new GlobalAppException("Invalid Enrollment data!");

            var entity = await _db.Enrollments
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (entity == null)
                throw new GlobalAppException("Enrollment not found!");

            // StudentId update (optional)
            if (!string.IsNullOrWhiteSpace(dto.StudentId))
            {
                if (!Guid.TryParse(dto.StudentId, out var newStudentId))
                    throw new GlobalAppException("Invalid Student ID!");

                if (newStudentId != entity.StudentId)
                {
                    var student = await _db.Students.FindAsync(newStudentId);
                    if (student == null)
                        throw new GlobalAppException("Student not found!");

                    // duplicate check with new student + current section
                    bool exists = await _db.Enrollments.AnyAsync(e =>
                        e.Id != enrollmentId &&
                        e.StudentId == newStudentId &&
                        e.SectionId == entity.SectionId);

                    if (exists)
                        throw new GlobalAppException("This student is already enrolled in this section!");

                    entity.StudentId = newStudentId;
                }
            }

            // SectionId update (optional)
            if (!string.IsNullOrWhiteSpace(dto.SectionId))
            {
                if (!Guid.TryParse(dto.SectionId, out var newSectionId))
                    throw new GlobalAppException("Invalid Section ID!");

                if (newSectionId != entity.SectionId)
                {
                    var section = await _db.Sections.FindAsync(newSectionId);
                    if (section == null)
                        throw new GlobalAppException("Section not found!");

                    // duplicate check with current student + new section
                    bool exists = await _db.Enrollments.AnyAsync(e =>
                        e.Id != enrollmentId &&
                        e.StudentId == entity.StudentId &&
                        e.SectionId == newSectionId);

                    if (exists)
                        throw new GlobalAppException("This student is already enrolled in this section!");

                    entity.SectionId = newSectionId;
                }
            }

            // Grades
            if (dto.TotalGrade.HasValue)
                entity.TotalGrade = dto.TotalGrade.Value;

            if (dto.LetterGrade != null) // allow clearing by sending null? if you want
                entity.LetterGrade = string.IsNullOrWhiteSpace(dto.LetterGrade) ? null : dto.LetterGrade;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var enrollmentId))
                throw new GlobalAppException("Invalid Enrollment ID!");

            var entity = await _db.Enrollments.FindAsync(enrollmentId);
            if (entity == null)
                throw new GlobalAppException("Enrollment not found!");

            _db.Enrollments.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}
