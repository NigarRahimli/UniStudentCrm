using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Account
{
    public class TokenResponseDto
    {
        public string Token { get; set; }
        public DateTime ExpireDate { get; set; }
        public string Role { get; set; }
        public bool MustChangePassword { get; set; }
    }
}
