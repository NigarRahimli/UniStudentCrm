using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;
using Student.Domain.HelperEntities;
using StudentCrm.Application.Abstract.Repositories.Coordinators;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Coordinator;
using StudentCrm.Application.GlobalAppException;
using StudentCrm.Application.Helpers;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class CoordinatorService : ICoordinatorService
    {
        private readonly ICoordinatorReadRepository _coordinatorReadRepo;
        private readonly ICoordinatorWriteRepository _coordinatorWriteRepo;

        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IMailService _mailService;

        public CoordinatorService(
            ICoordinatorReadRepository coordinatorReadRepo,
            ICoordinatorWriteRepository coordinatorWriteRepo,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IMapper mapper,
            IMailService mailService)
        {
            _coordinatorReadRepo = coordinatorReadRepo;
            _coordinatorWriteRepo = coordinatorWriteRepo;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _mailService = mailService;
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        public async Task<List<CoordinatorDto>> GetAllAsync()
        {
            var list = await _coordinatorReadRepo.GetAllAsync(
                c => !EF.Property<bool>(c, "IsDeleted"),
                include: q => q.Include(c => c.AppUser),
                enableTracking: false
            );

            return _mapper.Map<List<CoordinatorDto>>(list.ToList());
        }

        public async Task<CoordinatorDto> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var coordinatorId))
                throw new GlobalAppException("Invalid Coordinator ID!");

            var entity = await _coordinatorReadRepo.GetAsync(
                c => c.Id == coordinatorId && !EF.Property<bool>(c, "IsDeleted"),
                include: q => q.Include(c => c.AppUser),
                enableTracking: false
            );

            if (entity == null)
                throw new GlobalAppException("Coordinator not found!");

            return _mapper.Map<CoordinatorDto>(entity);
        }

        public async Task CreateAsync(CreateCoordinatorDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Submitted data is null!");
            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new GlobalAppException("FullName cannot be empty!");
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new GlobalAppException("Email cannot be empty!");

            var email = NormalizeEmail(dto.Email);

            // Unique CoordinatorNo (Domain)
            var noExists = await _coordinatorReadRepo.GetAsync(
                x => x.CoordinatorNo == dto.CoordinatorNo && !EF.Property<bool>(x, "IsDeleted")
            );
            if (noExists != null)
                throw new GlobalAppException("Coordinator number must be unique!");

            // Unique Email in Identity
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
                throw new GlobalAppException("This email is already used!");

            // Ensure role exists
            const string roleName = "Coordinator";
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleCreate = await _roleManager.CreateAsync(new AppRole { Name = roleName });
                if (!roleCreate.Succeeded)
                {
                    var errs = string.Join(", ", roleCreate.Errors.Select(e => e.Description));
                    throw new GlobalAppException($"Role creation failed: {errs}");
                }
            }

            // temp password (send after commit)
            var tempPassword = PasswordHelper.GenerateTempPassword(12);

            // ✅ Start transaction using DbContext from repository
            var db = _coordinatorWriteRepo.GetDbContext();
            await using var tx = await db.Database.BeginTransactionAsync();

            AppUser? createdUser = null;

            try
            {
                // 1) Create identity user
                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    MustChangePassword = true,
                    TemporaryPasswordIssuedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, tempPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new GlobalAppException($"Login creation failed: {errors}");
                }

                createdUser = user;

                // 2) Add role
                var addRole = await _userManager.AddToRoleAsync(user, roleName);
                if (!addRole.Succeeded)
                {
                    var errors = string.Join(", ", addRole.Errors.Select(e => e.Description));
                    throw new GlobalAppException($"Assign role failed: {errors}");
                }

                // 3) Create domain coordinator
                var coordinator = new CoordinatorUser
                {
                    FullName = dto.FullName.Trim(),
                    Department = dto.Department?.Trim(),
                    CoordinatorNo = dto.CoordinatorNo,
                    AppUserId = user.Id
                };

                await _coordinatorWriteRepo.AddAsync(coordinator);
                await _coordinatorWriteRepo.CommitAsync();

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();

                // compensate identity if created
                if (createdUser != null)
                    await _userManager.DeleteAsync(createdUser);

                throw;
            }

            // ✅ Send email AFTER commit (mail failure must not rollback DB)
            try
            {
                await _mailService.SendEmailAsync(new MailRequest
                {
                    ToEmail = email,
                    Subject = "StudentCRM - Coordinator hesabınız yaradıldı",
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
                // no rollback here; optionally log
            }
        }

        public async Task UpdateAsync(UpdateCoordinatorDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Invalid Coordinator data!");

            if (string.IsNullOrWhiteSpace(dto.Id) || !Guid.TryParse(dto.Id, out var coordinatorId))
                throw new GlobalAppException("Invalid Coordinator ID!");

            var existing = await _coordinatorReadRepo.GetAsync(
                c => c.Id == coordinatorId && !EF.Property<bool>(c, "IsDeleted"),
                include: q => q.Include(c => c.AppUser),
                enableTracking: true
            );

            if (existing == null)
                throw new GlobalAppException("Coordinator not found!");

            // unique CoordinatorNo (if changed)
            if (dto.CoordinatorNo.HasValue && dto.CoordinatorNo.Value != existing.CoordinatorNo)
            {
                var another = await _coordinatorReadRepo.GetAsync(x =>
                    x.CoordinatorNo == dto.CoordinatorNo.Value &&
                    x.Id != coordinatorId &&
                    !EF.Property<bool>(x, "IsDeleted"));

                if (another != null)
                    throw new GlobalAppException("Coordinator number must be unique!");

                existing.CoordinatorNo = dto.CoordinatorNo.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                existing.FullName = dto.FullName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Department))
                existing.Department = dto.Department.Trim();

            // email update (Identity)
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var newEmail = NormalizeEmail(dto.Email);

                var other = await _userManager.FindByEmailAsync(newEmail);
                if (other != null && other.Id != existing.AppUserId)
                    throw new GlobalAppException("This email is already used!");

                var user = existing.AppUser ?? await _userManager.FindByIdAsync(existing.AppUserId.ToString());
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
            }

            await _coordinatorWriteRepo.UpdateAsync(existing);
            await _coordinatorWriteRepo.CommitAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var coordinatorId))
                throw new GlobalAppException("Invalid Coordinator ID!");

            var entity = await _coordinatorReadRepo.GetAsync(
                c => c.Id == coordinatorId && !EF.Property<bool>(c, "IsDeleted"),
                enableTracking: true
            );

            if (entity == null)
                throw new GlobalAppException("Coordinator not found!");

            // If you prefer hard delete:
            // await _coordinatorWriteRepo.HardDeleteAsync(entity);

            // If you prefer soft delete (recommended since you already have it):
            await _coordinatorWriteRepo.SoftDeleteAsync(entity);

            await _coordinatorWriteRepo.CommitAsync();

            // Also remove Identity user
            var user = await _userManager.FindByIdAsync(entity.AppUserId.ToString());
            if (user != null)
                await _userManager.DeleteAsync(user);
        }

        public async Task ResetPasswordAndEmailAsync(string coordinatorId)
        {
            if (!Guid.TryParse(coordinatorId, out var cid))
                throw new GlobalAppException("Invalid Coordinator ID!");

            var coordinator = await _coordinatorReadRepo.GetAsync(
                c => c.Id == cid && !EF.Property<bool>(c, "IsDeleted"),
                include: q => q.Include(c => c.AppUser),
                enableTracking: false
            );

            if (coordinator == null)
                throw new GlobalAppException("Coordinator not found!");

            var user = coordinator.AppUser ?? await _userManager.FindByIdAsync(coordinator.AppUserId.ToString());
            if (user == null)
                throw new GlobalAppException("Related user account not found!");

            var email = user.Email;
            if (string.IsNullOrWhiteSpace(email))
                throw new GlobalAppException("Coordinator email not found!");

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
                Subject = "StudentCRM - Coordinator yeni müvəqqəti şifrə",
                Body = $@"
                    <h3>Salam {coordinator.FullName},</h3>
                    <p>Yeni müvəqqəti şifrə yaradıldı.</p>
                    <p><b>Login:</b> {email}</p>
                    <p><b>Müvəqqəti şifrə:</b> {newTempPassword}</p>
                "
            });
        }
    }
}
