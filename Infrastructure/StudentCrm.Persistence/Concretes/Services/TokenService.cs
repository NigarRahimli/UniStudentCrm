using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Account;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TokenResponseDto CreateToken(AppUser user, string role, int expireMinutes = 180)
        {
            var signingKey = _configuration["Jwt:SigningKey"];
            if (string.IsNullOrWhiteSpace(signingKey))
                throw new InvalidOperationException("Jwt:SigningKey appsettings-də yoxdur.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),   // ✅ Guid -> string
                new Claim(ClaimTypes.GivenName, user.Name ?? ""),
                new Claim(ClaimTypes.Surname, user.Surname ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),   // ✅ ClaimsIdentity burada lazımdır
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = creds
            };

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(tokenDescriptor);
            var token = handler.WriteToken(securityToken);

            return new TokenResponseDto
            {
                Token = token,
                ExpireDate = securityToken.ValidTo,
                Role = role
            };
        }
    }
}
