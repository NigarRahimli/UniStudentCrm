using Microsoft.AspNetCore.Identity;
using Student.Domain.Entities;
using Student.Domain.HelperEntities;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Account;
using StudentCrm.Application.GlobalAppException;
using StudentCrm.Application.Helpers;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IMailService _mailService;

        public AuthService(UserManager<AppUser> userManager, ITokenService tokenService, IMailService mailService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _mailService = mailService;
        }

        public async Task<TokenResponseDto> LoginAsync(LoginDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                throw new GlobalAppException("Email və şifrə boş ola bilməz!");

            var email = dto.Email.Trim().ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(email)
                       ?? throw new GlobalAppException("Email və ya şifrə yanlışdır!");

            var ok = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!ok)
                throw new GlobalAppException("Email və ya şifrə yanlışdır!");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            return _tokenService.CreateToken(user, role);
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new GlobalAppException("UserId boş ola bilməz!");

            if (dto == null || string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new GlobalAppException("Password məlumatları boş ola bilməz!");

            var user = await _userManager.FindByIdAsync(userId)
                       ?? throw new GlobalAppException("User tapılmadı!");

            // ✅ əvvəlcə current password yoxla (ChangePasswordAsync içində də yoxlayır, amma biz daha səliqəli mesaj üçün edirik)
            var correct = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
            if (!correct)
                throw new GlobalAppException("Cari şifrə yanlışdır!"); // <-- sənin istədiyin catch

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new GlobalAppException($"Şifrə dəyişmə uğursuz oldu: {errors}");
            }

            user.MustChangePassword = false;
            user.TemporaryPasswordIssuedAt = null;

            var upd = await _userManager.UpdateAsync(user);
            if (!upd.Succeeded)
            {
                var errors = string.Join(", ", upd.Errors.Select(e => e.Description));
                throw new GlobalAppException($"User update failed: {errors}");
            }
        }


        public async Task ResetPasswordAndEmailAsync(ResetPasswordRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
                throw new GlobalAppException("Email boş ola bilməz!");

            var email = dto.Email.Trim().ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new GlobalAppException("Bu email ilə user tapılmadı!");

            // ✅ Əgər user.Email null-dursa, mail göndərmək olmaz
            if (string.IsNullOrWhiteSpace(user.Email))
                throw new GlobalAppException("Bu user-in email-i boşdur (AspNetUsers.Email). Mail göndərilə bilməz!");

            var tempPassword = PasswordHelper.GenerateTempPassword(12);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await _userManager.ResetPasswordAsync(user, token, tempPassword);

            if (!reset.Succeeded)
            {
                var errors = string.Join(", ", reset.Errors.Select(e => e.Description));
                throw new GlobalAppException($"Password reset failed: {errors}");
            }

            user.MustChangePassword = true;
            user.TemporaryPasswordIssuedAt = DateTime.UtcNow;

            var upd = await _userManager.UpdateAsync(user);
            if (!upd.Succeeded)
            {
                var errors = string.Join(", ", upd.Errors.Select(e => e.Description));
                throw new GlobalAppException($"User update failed: {errors}");
            }

            // ✅ Mail göndər (artıq email boş deyil)
            await _mailService.SendEmailAsync(new MailRequest
            {
                ToEmail = user.Email!,
                Subject = "StudentCRM - Yeni müvəqqəti şifrə",
                Body = $@"
                    <h3>Salam,</h3>
                    <p>Yeni müvəqqəti şifrə yaradıldı.</p>
                    <p><b>Login:</b> {user.Email}</p>
                    <p><b>Müvəqqəti şifrə:</b> {tempPassword}</p>
                    <p>İlk girişdə şifrənizi dəyişməlisiniz.</p>
                "
            });
        }
    }
}
