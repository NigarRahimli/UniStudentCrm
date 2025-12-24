using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Repositories.Enrollments;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Enrollment;
using StudentCrm.Application.GlobalAppException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentReadRepository _enrollmentReadRepo;
        private readonly IEnrollmentWriteRepository _enrollmentWriteRepo;
        private readonly IStudentService _studentService;
        private readonly ISectionService _sectionService;
        private readonly IMapper _mapper;

        private const int LetterGradeMaxLength = 2;

        public EnrollmentService(
            IEnrollmentReadRepository enrollmentReadRepo,
            IEnrollmentWriteRepository enrollmentWriteRepo,
            IStudentService studentService,
            ISectionService sectionService,
            IMapper mapper)
        {
            _enrollmentReadRepo = enrollmentReadRepo;
            _enrollmentWriteRepo = enrollmentWriteRepo;
            _studentService = studentService;
            _sectionService = sectionService;
            _mapper = mapper;
        }

        public async Task<List<EnrollmentDto>> GetAllAsync()
        {
            var enrollments = await _enrollmentReadRepo.GetAllAsync(
                e => !EF.Property<bool>(e, "IsDeleted"),
                include: q => q
                    .Include(e => e.Student)
                        .ThenInclude(s => s.AppUser) // so Student email can map from AppUser if needed
                    .Include(e => e.Section)
                        .ThenInclude(s => s.Course)
                    .Include(e => e.Section)
                        .ThenInclude(s => s.Term)
                    .Include(e => e.Section)
                        .ThenInclude(s => s.Teacher),
                enableTracking: false
            );

            return _mapper.Map<List<EnrollmentDto>>(enrollments.ToList());
        }

        public async Task<EnrollmentDto> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var enrollmentId))
                throw new GlobalAppException("Invalid Enrollment ID!");

            var enrollment = await _enrollmentReadRepo.GetAsync(
                e => e.Id == enrollmentId && !EF.Property<bool>(e, "IsDeleted"),
                include: q => q
                    .Include(e => e.Student)
                        .ThenInclude(s => s.AppUser)
                    .Include(e => e.Section)
                        .ThenInclude(s => s.Course)
                    .Include(e => e.Section)
                        .ThenInclude(s => s.Term)
                    .Include(e => e.Section)
                        .ThenInclude(s => s.Teacher),
                enableTracking: false
            );

            if (enrollment == null)
                throw new GlobalAppException("Enrollment not found!");

            return _mapper.Map<EnrollmentDto>(enrollment);
        }

        public async Task CreateAsync(CreateEnrollmentDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Göndərilən məlumat boşdur!");

            if (string.IsNullOrWhiteSpace(dto.StudentId) || !Guid.TryParse(dto.StudentId, out var studentId))
                throw new GlobalAppException("Yanlış Student ID!");

            if (string.IsNullOrWhiteSpace(dto.SectionId) || !Guid.TryParse(dto.SectionId, out var sectionId))
                throw new GlobalAppException("Yanlış Section ID!");

            if (dto.TotalGrade is not null && (dto.TotalGrade < 0 || dto.TotalGrade > 100))
                throw new GlobalAppException("TotalGrade 0-100 aralığında olmalıdır!");

            if (!string.IsNullOrWhiteSpace(dto.LetterGrade) && dto.LetterGrade.Trim().Length > 2)
                throw new GlobalAppException("LetterGrade maksimum 2 simvol ola bilər!");

            // validate via services (your architecture)
            await _studentService.GetByIdAsync(dto.StudentId);
            await _sectionService.GetByIdAsync(dto.SectionId);

            // 1) ACTIVE duplicate?
            var active = await _enrollmentReadRepo.GetAsync(e =>
                e.StudentId == studentId &&
                e.SectionId == sectionId &&
                EF.Property<bool>(e, "IsDeleted") == false);

            if (active != null)
                throw new GlobalAppException("Bu tələbə artıq bu section-a enroll olunub!");

            // 2) SOFT-DELETED duplicate? -> restore
            var deleted = await _enrollmentReadRepo.GetAsync(e =>
                e.StudentId == studentId &&
                e.SectionId == sectionId &&
                EF.Property<bool>(e, "IsDeleted") == true,
                enableTracking: true);

            if (deleted != null)
            {
                // restore: ONLY set IsDeleted=false (DON'T set DeletedDate=null because your DeletedDate is required)
                var db = _enrollmentWriteRepo.GetDbContext();
                db.Attach(deleted);

                var entry = db.Entry(deleted);
                entry.Property("IsDeleted").CurrentValue = false;
                entry.Property("IsDeleted").IsModified = true;

                // DO NOT do DeletedDate = null (it is required in your model/db)

                deleted.TotalGrade = dto.TotalGrade;
                deleted.LetterGrade = string.IsNullOrWhiteSpace(dto.LetterGrade) ? null : dto.LetterGrade.Trim();

                await _enrollmentWriteRepo.UpdateAsync(deleted);
                await _enrollmentWriteRepo.CommitAsync();
                return;
            }

            // 3) create new
            var enrollment = new Enrollment
            {
                StudentId = studentId,
                SectionId = sectionId,
                TotalGrade = dto.TotalGrade,
                LetterGrade = string.IsNullOrWhiteSpace(dto.LetterGrade) ? null : dto.LetterGrade.Trim()
            };

            await _enrollmentWriteRepo.AddAsync(enrollment);
            await _enrollmentWriteRepo.CommitAsync();
        }

        public async Task UpdateAsync(UpdateEnrollmentDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Göndərilən məlumat boşdur!");

            if (string.IsNullOrWhiteSpace(dto.Id) || !Guid.TryParse(dto.Id, out var enrollmentId))
                throw new GlobalAppException("Yanlış Enrollment ID!");

            var enrollment = await _enrollmentReadRepo.GetAsync(
                e => e.Id == enrollmentId && !EF.Property<bool>(e, "IsDeleted"),
                enableTracking: true
            );

            if (enrollment == null)
                throw new GlobalAppException("Enrollment tapılmadı!");

            // keep old values
            var oldStudentId = enrollment.StudentId;
            var oldSectionId = enrollment.SectionId;
            var oldTotalGrade = enrollment.TotalGrade;
            var oldLetterGrade = enrollment.LetterGrade;

            if (dto.StudentId != null)
            {
                if (string.IsNullOrWhiteSpace(dto.StudentId) || !Guid.TryParse(dto.StudentId, out var newStudentId))
                    throw new GlobalAppException("Yanlış Student ID!");

                await _studentService.GetByIdAsync(dto.StudentId);
                enrollment.StudentId = newStudentId;
            }
            else enrollment.StudentId = oldStudentId;

            if (dto.SectionId != null)
            {
                if (string.IsNullOrWhiteSpace(dto.SectionId) || !Guid.TryParse(dto.SectionId, out var newSectionId))
                    throw new GlobalAppException("Yanlış Section ID!");

                await _sectionService.GetByIdAsync(dto.SectionId);
                enrollment.SectionId = newSectionId;
            }
            else enrollment.SectionId = oldSectionId;

            if (dto.TotalGrade != null)
            {
                if (dto.TotalGrade < 0 || dto.TotalGrade > 100)
                    throw new GlobalAppException("TotalGrade 0-100 aralığında olmalıdır!");

                enrollment.TotalGrade = dto.TotalGrade;
            }
            else enrollment.TotalGrade = oldTotalGrade;

            if (dto.LetterGrade != null)
            {
                var trimmed = dto.LetterGrade.Trim();

                if (!string.IsNullOrWhiteSpace(trimmed) && trimmed.Length > LetterGradeMaxLength)
                    throw new GlobalAppException($"LetterGrade maksimum {LetterGradeMaxLength} simvol ola bilər!");

                enrollment.LetterGrade = string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
            }
            else enrollment.LetterGrade = oldLetterGrade;

            // prevent duplicates after update (active ones)
            var dupActive = await _enrollmentReadRepo.GetAsync(e =>
                e.Id != enrollmentId &&
                e.StudentId == enrollment.StudentId &&
                e.SectionId == enrollment.SectionId &&
                !EF.Property<bool>(e, "IsDeleted"));

            if (dupActive != null)
                throw new GlobalAppException("Bu tələbə artıq bu section-a enroll olunub!");

            await _enrollmentWriteRepo.UpdateAsync(enrollment);
            await _enrollmentWriteRepo.CommitAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var enrollmentId))
                throw new GlobalAppException("Yanlış Enrollment ID!");

            var enrollment = await _enrollmentReadRepo.GetAsync(
                e => e.Id == enrollmentId && !EF.Property<bool>(e, "IsDeleted"),
                enableTracking: true
            );

            if (enrollment == null)
                throw new GlobalAppException("Enrollment tapılmadı!");

            await _enrollmentWriteRepo.SoftDeleteAsync(enrollment);
            await _enrollmentWriteRepo.CommitAsync();
        }
    }
}
