using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Account;
using StudentCrm.Application.GlobalAppException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AdminService> _logger;

        public AdminService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, IMapper mapper, ITokenService tokenService, ILogger<AdminService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _tokenService = tokenService;
            _logger = logger;
        }

        // İstifadəçi qeydiyyatı metodu
        public async Task RegisterAdminAsync(RegisterDto registerDto)
        {
            var admin = _mapper.Map<AppUser>(registerDto);

            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
                throw new GlobalAppException("Bu e-poçt ilə artıq bir istifadəçi mövcuddur!");

            var result = await _userManager.CreateAsync(admin, registerDto.Password);
            if (!result.Succeeded)
                throw new GlobalAppException($"İstifadəçi yaradılmadı: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            const string adminRole = "Admin";

            // ✅ Role yoxdursa yarat: AppRole istifadə et
            if (!await _roleManager.RoleExistsAsync(adminRole))
            {
                var roleCreate = await _roleManager.CreateAsync(new AppRole { Name = adminRole });
                if (!roleCreate.Succeeded)
                    throw new GlobalAppException($"Role yaradılmadı: {string.Join(", ", roleCreate.Errors.Select(e => e.Description))}");
            }

            var addToRole = await _userManager.AddToRoleAsync(admin, adminRole);
            if (!addToRole.Succeeded)
                throw new GlobalAppException($"Role təyin edilmədi: {string.Join(", ", addToRole.Errors.Select(e => e.Description))}");
        }


        // İstifadəçi giriş metodu
        public async Task<TokenResponseDto> LoginAdminAsync(LoginDto dto)
        {
            try
            {
                var user = await _userManager.Users
                    .Where(u => (u.Email == dto.Email))
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    throw new GlobalAppException("Daxil edilən email ilə admin tapılmadı.");
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
                if (!isPasswordValid)
                {
                    throw new GlobalAppException("Şifrə yalnışdır.");
                }

                var roles = await _userManager.GetRolesAsync(user);

                if (!roles.Any(r => r == "Admin"))
                {
                    throw new GlobalAppException("Giriş rədd edildi! Yalnız Admin  daxil ola bilər.");
                }

                var userRole = roles.FirstOrDefault();

                var tokenResponse = _tokenService.CreateToken(user, userRole);

                return tokenResponse;
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İstifadəçi daxil edilərkən bir səhv baş verdi");
                throw new GlobalAppException("İstifadəçi daxil edilərkən gözlənilməz bir səhv baş verdi", ex);
            }
        }

        // Admin silmə metodu
        public async Task DeleteAllAdminsAsync()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            if (admins == null || !admins.Any())
            {
                throw new GlobalAppException("Silmək üçün heç bir admin tapılmadı!");
            }

            foreach (var admin in admins)
            {
                var result = await _userManager.DeleteAsync(admin);
                if (!result.Succeeded)
                {
                    throw new GlobalAppException($"Admin {admin.Email} silinərkən xəta baş verdi: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

    }
}
