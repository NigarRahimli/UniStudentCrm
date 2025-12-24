using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;
using Student.Domain.HelperEntities;
using StudentCrm.Application.Abstract.Repositories.Students;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Student;
using StudentCrm.Application.GlobalAppException;
using StudentCrm.Application.Helpers;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentReadRepository _studentReadRepo;
        private readonly IStudentWriteRepository _studentWriteRepo;

        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMailService _mailService;

        public StudentService(
            IStudentReadRepository studentReadRepo,
            IStudentWriteRepository studentWriteRepo,
            IMapper mapper,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IMailService mailService)
        {
            _studentReadRepo = studentReadRepo;
            _studentWriteRepo = studentWriteRepo;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _mailService = mailService;
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        // -------------------------
        // Queries
        // -------------------------
        public async Task<List<StudentDto>> GetAllAsync()
        {
            var students = await _studentReadRepo.GetAllAsync(
                s => !EF.Property<bool>(s, "IsDeleted"),
                include: q => q.Include(s => s.AppUser),
                enableTracking: false
            );

            return _mapper.Map<List<StudentDto>>(students.ToList());
        }

        public async Task<StudentDetailDto> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var studentId))
                throw new GlobalAppException("Invalid student ID!");

            var student = await _studentReadRepo.GetAsync(
                s => s.Id == studentId && !EF.Property<bool>(s, "IsDeleted"),
                include: q => q
                    .Include(s => s.AppUser)
                    .Include(s => s.Enrollments)
                        .ThenInclude(e => e.Section),
                enableTracking: false
            );

            if (student == null)
                throw new GlobalAppException("Student not found!");

            return _mapper.Map<StudentDetailDto>(student);
        }

        // -------------------------
        // Create (Identity user + temp password + role + Student profile)
        // Email is sent AFTER commit (no rollback after commit)
        // -------------------------
        public async Task CreateAsync(CreateStudentDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Submitted data is null!");

            if (string.IsNullOrWhiteSpace(dto.StudentNo))
                throw new GlobalAppException("StudentNo cannot be empty!");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new GlobalAppException("Name cannot be empty!");

            if (string.IsNullOrWhiteSpace(dto.Surname))
                throw new GlobalAppException("Surname cannot be empty!");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new GlobalAppException("Email cannot be empty!");

            var email = NormalizeEmail(dto.Email);
            var studentNo = dto.StudentNo.Trim();

            // Unique StudentNo (Domain)
            var noExists = await _studentReadRepo.GetAsync(s =>
                s.StudentNo == studentNo && !EF.Property<bool>(s, "IsDeleted"));

            if (noExists != null)
                throw new GlobalAppException("StudentNo must be unique!");

            // Unique Email in Identity
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
                throw new GlobalAppException("This email is already used!");

            // Ensure role exists
            const string roleName = "Student";
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleCreate = await _roleManager.CreateAsync(new AppRole { Name = roleName });
                if (!roleCreate.Succeeded)
                {
                    var errs = string.Join(", ", roleCreate.Errors.Select(e => e.Description));
                    throw new GlobalAppException($"Role creation failed: {errs}");
                }
            }

            // Prepare password once (email after commit)
            var tempPassword = PasswordHelper.GenerateTempPassword(12);

            AppUser? createdUser = null;

            // DB transaction only for DB operations (use DbContext from repository)
            var db = _studentWriteRepo.GetDbContext();
            await using var tx = await db.Database.BeginTransactionAsync();

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
                    throw new GlobalAppException($"User creation failed: {errors}");
                }

                createdUser = user;

                var addRoleResult = await _userManager.AddToRoleAsync(user, roleName);
                if (!addRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                    throw new GlobalAppException($"Assign role failed: {errors}");
                }

                // 2) Create Student profile
                var student = new StudentUser
                {
                    StudentNo = studentNo,
                    Name = dto.Name.Trim(),
                    Surname = dto.Surname.Trim(),
                    Phone = dto.Phone?.Trim(),
                    Email = email, // optional sync
                    Major = dto.Major?.Trim(),
                    GPA = dto.GPA,
                    AppUserId = user.Id
                };

                await _studentWriteRepo.AddAsync(student);
                await _studentWriteRepo.CommitAsync();

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();

                // Compensate Identity user if created
                if (createdUser != null)
                    await _userManager.DeleteAsync(createdUser);

                throw;
            }

            // Email AFTER commit. If mail fails => no rollback.
            try
            {
                await _mailService.SendEmailAsync(new MailRequest
                {
                    ToEmail = email,
                    Subject = "StudentCRM - Hesabınız yaradıldı",
                    Body = $@"
                        <h3>Salam {dto.Name.Trim()} {dto.Surname.Trim()},</h3>
                        <p>Hesabınız yaradıldı.</p>
                        <p><b>Login:</b> {email}</p>
                        <p><b>Müvəqqəti şifrə:</b> {tempPassword}</p>
                        <p>İlk girişdən sonra şifrənizi dəyişməyiniz tövsiyə olunur.</p>
                    "
                });
            }
            catch
            {
                // ideally log; do not throw
            }
        }

        // -------------------------
        // Update (profile; if Email changed -> update Identity user too)
        // -------------------------
        public async Task UpdateAsync(UpdateStudentDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Invalid student update data!");

            if (string.IsNullOrWhiteSpace(dto.Id) || !Guid.TryParse(dto.Id, out var studentId))
                throw new GlobalAppException("Invalid student ID!");

            var student = await _studentReadRepo.GetAsync(
                s => s.Id == studentId && !EF.Property<bool>(s, "IsDeleted"),
                include: q => q.Include(s => s.AppUser),
                enableTracking: true
            );

            if (student == null)
                throw new GlobalAppException("Student not found!");

            if (!string.IsNullOrWhiteSpace(dto.StudentNo))
            {
                var newNo = dto.StudentNo.Trim();

                var exists = await _studentReadRepo.GetAsync(s =>
                    s.StudentNo == newNo &&
                    s.Id != studentId &&
                    !EF.Property<bool>(s, "IsDeleted"));

                if (exists != null)
                    throw new GlobalAppException("Another student with this StudentNo already exists!");

                student.StudentNo = newNo;
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                student.Name = dto.Name.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Surname))
                student.Surname = dto.Surname.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                student.Phone = dto.Phone.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Major))
                student.Major = dto.Major.Trim();

            if (dto.GPA != null)
                student.GPA = dto.GPA;

            // Email change => update Identity too
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var newEmail = NormalizeEmail(dto.Email);

                if (!string.Equals(student.AppUser?.Email, newEmail, StringComparison.OrdinalIgnoreCase))
                {
                    var other = await _userManager.FindByEmailAsync(newEmail);
                    if (other != null && other.Id != student.AppUserId)
                        throw new GlobalAppException("This email is already used!");

                    var user = student.AppUser ?? await _userManager.FindByIdAsync(student.AppUserId.ToString());
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

                    student.Email = newEmail; // optional sync
                }
            }

            await _studentWriteRepo.UpdateAsync(student);
            await _studentWriteRepo.CommitAsync();
        }

        // -------------------------
        // Delete (soft delete student + remove Identity user)
        // -------------------------
        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var studentId))
                throw new GlobalAppException("Invalid student ID!");

            var student = await _studentReadRepo.GetAsync(
                s => s.Id == studentId && !EF.Property<bool>(s, "IsDeleted"),
                enableTracking: true
            );

            if (student == null)
                throw new GlobalAppException("Student not found!");

            // DB transaction for DB ops; identity delete after (like your current pattern)
            var db = _studentWriteRepo.GetDbContext();
            await using var tx = await db.Database.BeginTransactionAsync();

            try
            {
                // Soft delete (recommended). If you want hard delete, use HardDeleteAsync.
                await _studentWriteRepo.SoftDeleteAsync(student);
                await _studentWriteRepo.CommitAsync();

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            var user = await _userManager.FindByIdAsync(student.AppUserId.ToString());
            if (user != null)
            {
                var del = await _userManager.DeleteAsync(user);
                if (!del.Succeeded)
                {
                    var errors = string.Join(", ", del.Errors.Select(e => e.Description));
                    throw new GlobalAppException($"User delete failed: {errors}");
                }
            }
        }

        // -------------------------
        // Reset password + email
        // -------------------------
        public async Task ResetPasswordAndEmailAsync(string studentId)
        {
            if (!Guid.TryParse(studentId, out var sid))
                throw new GlobalAppException("Invalid student ID!");

            var student = await _studentReadRepo.GetAsync(
                s => s.Id == sid && !EF.Property<bool>(s, "IsDeleted"),
                include: q => q.Include(s => s.AppUser),
                enableTracking: false
            );

            if (student == null)
                throw new GlobalAppException("Student not found!");

            var user = student.AppUser ?? await _userManager.FindByIdAsync(student.AppUserId.ToString());
            if (user == null)
                throw new GlobalAppException("Related user account not found!");

            var email = user.Email ?? student.Email;
            if (string.IsNullOrWhiteSpace(email))
                throw new GlobalAppException("Student email not found!");

            var newTempPassword = PasswordHelper.GenerateTempPassword(12);

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, newTempPassword);

            if (!resetResult.Succeeded)
            {
                var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                throw new GlobalAppException($"Password reset failed: {errors}");
            }

            user.MustChangePassword = true;
            user.TemporaryPasswordIssuedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await _mailService.SendEmailAsync(new MailRequest
            {
                ToEmail = email,
                Subject = "StudentCRM - Yeni müvəqqəti şifrə",
                Body = $@"
                    <h3>Salam {student.Name} {student.Surname},</h3>
                    <p>Hesabınız üçün yeni müvəqqəti şifrə yaradıldı.</p>
                    <p><b>Login:</b> {email}</p>
                    <p><b>Müvəqqəti şifrə:</b> {newTempPassword}</p>
                    <p>İlk girişdən sonra şifrənizi dəyişin.</p>
                "
            });
        }
    }
}
