using Student.Domain.Entities;
using StudentCrm.Application.DTOs.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.Abstract.Services
{
    public interface ITokenService
    {
        TokenResponseDto CreateToken(AppUser admin, string role, int expireDate = 1440);

    }
}
