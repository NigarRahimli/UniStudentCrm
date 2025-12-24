using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Account;
using System.Security.Claims;

namespace StudentCrm.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(new { statusCode = 200, data = result });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _authService.ChangePasswordAsync(userId!, dto);
            return Ok(new { statusCode = 200, message = "Password changed successfully." });
        }

        // Admin resets any user's password by email
        [HttpPost("reset-password")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            await _authService.ResetPasswordAndEmailAsync(dto);
            return Ok(new { statusCode = 200, message = "Temporary password sent." });
        }
    }
}
