using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;
using Student.Domain.HelperEntities;
using StudentCrm.Application.Abstract.Repositories.Sections;
using StudentCrm.Application.Abstract.Repositories.Teachers;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Teacher;
using StudentCrm.Application.GlobalAppException;
using StudentCrm.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherReadRepository _teacherReadRepo;
        private readonly ITeacherWriteRepository _teacherWriteRepo;
        private readonly ISectionReadRepository _sectionReadRepo;
        private readonly ISectionWriteRepository _sectionWriteRepo;

        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IMailService _mailService;

        public TeacherService(
            ITeacherReadRepository teacherReadRepo,
            ITeacherWriteRepository teacherWriteRepo,
            ISectionReadRepository sectionReadRepo,
            ISectionWriteRepository sectionWriteRepo,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IMapper mapper,
            IMailService mailService)
        {
            _teacherReadRepo = teacherReadRepo;
            _teacherWriteRepo = teacherWriteRepo;
            _sectionReadRepo = sectionReadRepo;
            _sectionWriteRepo = sectionWriteRepo;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _mailService = mailService;
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        public async Task<List<TeacherDto>> GetAllAsync()
        {
            var teachers = await _teacherReadRepo.GetAllAsync(
                t => !EF.Property<bool>(t, "IsDeleted"),
                include: q => q
                    .Include(t => t.Sections).ThenInclude(s => s.Course)
                    .Include(t => t.Sections).ThenInclude(s => s.Term),
                enableTracking: false
            );

            return _mapper.Map<List<TeacherDto>>(teachers.ToList());
        }

        public async Task<TeacherDto> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var teacherId))
                throw new GlobalAppException("Invalid teacher ID!");

            var teacher = await _teacherReadRepo.GetAsync(
                t => t.Id == teacherId && !EF.Property<bool>(t, "IsDeleted"),
                include: q => q
                    .Include(t => t.Sections).ThenInclude(s => s.Course)
                    .Include(t => t.Sections).ThenInclude(s => s.Term),
                enableTracking: false
            );

            if (teacher == null)
                throw new GlobalAppException("Teacher not found!");

            return _mapper.Map<TeacherDto>(teacher);
        }

        public async Task CreateAsync(CreateTeacherDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Submitted data is null!");
            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new GlobalAppException("FullName cannot be empty!");
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new GlobalAppException("Email cannot be empty!");

            // Unique StaffNo (Domain)
            var staffNoExists = await _teacherReadRepo.GetAsync(t =>
                t.StaffNo == dto.StaffNo && !EF.Property<bool>(t, "IsDeleted"));

            if (staffNoExists != null)
                throw new GlobalAppException("A teacher with this StaffNo already exists!");

            var email = NormalizeEmail(dto.Email);

            // Unique Email in Identity
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
                throw new GlobalAppException("This email is already used!");

            const string roleName = "Teacher";
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleCreate = await _roleManager.CreateAsync(new AppRole { Name = roleName });
                if (!roleCreate.Succeeded)
                {
                    var errs = string.Join(", ", roleCreate.Errors.Select(e => e.Description));
                    throw new GlobalAppException($"Role creation failed: {errs}");
                }
            }

            // password outside tx (needed for mail after commit)
            var tempPassword = PasswordHelper.GenerateTempPassword(12);

            // start tx from dbcontext (via repository)
            var db = _teacherWriteRepo.GetDbContext();
            await using var tx = await db.Database.BeginTransactionAsync();

            AppUser? createdUser = null;
            TeacherUser? createdTeacher = null;

            try
            {
                // 1) Create Identity user
                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    MustChangePassword = true,
                    TemporaryPasswordIssuedAt = DateTime.UtcNow
                };

                var createUserResult = await _userManager.CreateAsync(user, tempPassword);
                if (!createUserResult.Succeeded)
                {
                    var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                    throw new GlobalAppException($"Login creation failed: {errors}");
                }

                createdUser = user;

                var addRoleResult = await _userManager.AddToRoleAsync(user, roleName);
                if (!addRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                    throw new GlobalAppException($"Assign role failed: {errors}");
                }

                // 2) Save teacher profile
                var teacher = new TeacherUser
                {
                    StaffNo = dto.StaffNo,
                    FullName = dto.FullName.Trim(),
                    Email = email,
                    AppUserId = user.Id
                };

                await _teacherWriteRepo.AddAsync(teacher);
                await _teacherWriteRepo.CommitAsync();

                createdTeacher = teacher;

                // 3) Link sections (optional)
                if (dto.SectionIds != null && dto.SectionIds.Any())
                {
                    foreach (var secIdStr in dto.SectionIds)
                    {
                        if (!Guid.TryParse(secIdStr, out var secId))
                            throw new GlobalAppException($"Invalid Section ID: {secIdStr}");

                        var section = await _sectionReadRepo.GetAsync(
                            s => s.Id == secId && !EF.Property<bool>(s, "IsDeleted"),
                            enableTracking: true
                        );

                        if (section == null)
                            throw new GlobalAppException($"Section with ID {secIdStr} not found!");

                        section.TeacherId = teacher.Id;
                        await _sectionWriteRepo.UpdateAsync(section);
                    }

                    await _sectionWriteRepo.CommitAsync();
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();

                // compensate identity if created
                if (createdUser != null)
                    await _userManager.DeleteAsync(createdUser);

                // (optional) if teacher was created but identity failed later, it will be rolled back by tx anyway

                throw;
            }

            // Email AFTER commit
            try
            {
                await _mailService.SendEmailAsync(new MailRequest
                {
                    ToEmail = email,
                    Subject = "StudentCRM - Teacher hesabınız yaradıldı",
                    Body = $@"
                        <h3>Salam {dto.FullName.Trim()},</h3>
                        <p>Hesabınız yaradıldı.</p>
                        <p><b>Login:</b> {email}</p>
                        <p><b>Müvəqqəti şifrə:</b> {tempPassword}</p>
                        <p>İlk girişdən sonra şifrənizi dəyişməyiniz tövsiyə olunur.</p>
                    "
                });
            }
            catch
            {
                // no rollback; optionally log
            }
        }

        public async Task UpdateAsync(UpdateTeacherDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Id) || !Guid.TryParse(dto.Id, out var teacherId))
                throw new GlobalAppException("Invalid teacher data!");

            var teacher = await _teacherReadRepo.GetAsync(
                t => t.Id == teacherId && !EF.Property<bool>(t, "IsDeleted"),
                include: q => q.Include(t => t.Sections),
                enableTracking: true
            );

            if (teacher == null)
                throw new GlobalAppException("Teacher not found!");

            // StaffNo update (if changed)
            if (dto.StaffNo != 0 && dto.StaffNo != teacher.StaffNo)
            {
                var exists = await _teacherReadRepo.GetAsync(t =>
                    t.StaffNo == dto.StaffNo &&
                    t.Id != teacher.Id &&
                    !EF.Property<bool>(t, "IsDeleted"));

                if (exists != null)
                    throw new GlobalAppException("Another teacher with this StaffNo already exists!");

                teacher.StaffNo = dto.StaffNo;
            }

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                teacher.FullName = dto.FullName.Trim();

            // update email (Identity + optional TeacherUser.Email)
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var newEmail = NormalizeEmail(dto.Email);

                var other = await _userManager.FindByEmailAsync(newEmail);
                if (other != null && other.Id != teacher.AppUserId)
                    throw new GlobalAppException("This email is already used!");

                var user = await _userManager.FindByIdAsync(teacher.AppUserId.ToString());
                if (user == null)
                    throw new GlobalAppException("Related user account not found!");

                user.Email = newEmail;
                user.UserName = newEmail;

                var upd = await _userManager.UpdateAsync(user);
                if (!upd.Succeeded)
                {
                    var errors = string.Join(", ", upd.Errors.Select(e => e.Description));
                    throw new GlobalAppException($"Email update failed: {errors}");
                }

                teacher.Email = newEmail;
            }

            // section links update (your logic)
            if (dto.SectionIds != null)
            {
                var incoming = dto.SectionIds.ToHashSet();

                // unlink removed
                foreach (var existingSec in teacher.Sections.ToList())
                {
                    if (!incoming.Contains(existingSec.Id.ToString()))
                    {
                        // TeacherId is Guid (non-nullable) in your model. Your current strategy used Guid.Empty.
                        existingSec.TeacherId = Guid.Empty;
                        await _sectionWriteRepo.UpdateAsync(existingSec);
                    }
                }

                // link added
                foreach (var secIdStr in dto.SectionIds)
                {
                    if (!Guid.TryParse(secIdStr, out var secId))
                        throw new GlobalAppException($"Invalid Section ID: {secIdStr}");

                    if (!teacher.Sections.Any(s => s.Id == secId))
                    {
                        var section = await _sectionReadRepo.GetAsync(
                            s => s.Id == secId && !EF.Property<bool>(s, "IsDeleted"),
                            enableTracking: true
                        );

                        if (section == null)
                            throw new GlobalAppException($"Section with ID {secIdStr} not found!");

                        section.TeacherId = teacher.Id;
                        await _sectionWriteRepo.UpdateAsync(section);
                    }
                }
            }

            await _teacherWriteRepo.UpdateAsync(teacher);
            await _teacherWriteRepo.CommitAsync();

            // if we updated sections above, commit those too
            if (dto.SectionIds != null)
                await _sectionWriteRepo.CommitAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var teacherId))
                throw new GlobalAppException("Invalid teacher ID!");

            var teacher = await _teacherReadRepo.GetAsync(
                t => t.Id == teacherId && !EF.Property<bool>(t, "IsDeleted"),
                enableTracking: true
            );

            if (teacher == null)
                throw new GlobalAppException("Teacher not found!");

            // Soft delete teacher (recommended)
            await _teacherWriteRepo.SoftDeleteAsync(teacher);
            await _teacherWriteRepo.CommitAsync();

            var user = await _userManager.FindByIdAsync(teacher.AppUserId.ToString());
            if (user != null)
                await _userManager.DeleteAsync(user);
        }

        public async Task ResetPasswordAndEmailAsync(string teacherId)
        {
            if (!Guid.TryParse(teacherId, out var tid))
                throw new GlobalAppException("Invalid teacher ID!");

            var teacher = await _teacherReadRepo.GetAsync(
                t => t.Id == tid && !EF.Property<bool>(t, "IsDeleted"),
                include: q => q.Include(t => t.AppUser),
                enableTracking: false
            );

            if (teacher == null)
                throw new GlobalAppException("Teacher not found!");

            var user = teacher.AppUser ?? await _userManager.FindByIdAsync(teacher.AppUserId.ToString());
            if (user == null)
                throw new GlobalAppException("Related user account not found!");

            var email = user.Email ?? teacher.Email;
            if (string.IsNullOrWhiteSpace(email))
                throw new GlobalAppException("Teacher email not found!");

            var newTempPassword = PasswordHelper.GenerateTempPassword(12);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var res = await _userManager.ResetPasswordAsync(user, token, newTempPassword);

            if (!res.Succeeded)
            {
                var errors = string.Join(", ", res.Errors.Select(e => e.Description));
                throw new GlobalAppException($"Password reset failed: {errors}");
            }

            user.MustChangePassword = true;
            user.TemporaryPasswordIssuedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await _mailService.SendEmailAsync(new MailRequest
            {
                ToEmail = email,
                Subject = "StudentCRM - Teacher yeni müvəqqəti şifrə",
                Body = $@"
                    <h3>Salam {teacher.FullName},</h3>
                    <p>Yeni müvəqqəti şifrə yaradıldı.</p>
                    <p><b>Login:</b> {email}</p>
                    <p><b>Müvəqqəti şifrə:</b> {newTempPassword}</p>
                "
            });
        }
    }
}
