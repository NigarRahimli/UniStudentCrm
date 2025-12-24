using StudentCrm.Application.DTOs.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.Abstract.Services
{
    public interface IAuthService
    {
        Task<TokenResponseDto> LoginAsync(LoginDto dto);
        Task ChangePasswordAsync(string userId, ChangePasswordDto dto);

        // Admin action (or system action) - reset and email a temp password:
        Task ResetPasswordAndEmailAsync(ResetPasswordRequestDto dto);

    }
}
