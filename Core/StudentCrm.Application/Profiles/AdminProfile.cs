using AutoMapper;
using Student.Domain.Entities;
using StudentCrm.Application.DTOs.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.Profiles
{
    public class AdminProfile : Profile
    {
        public AdminProfile()
        {
            // RegisterDto -> Admin
            CreateMap<RegisterDto, AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)) // Kullanıcı adı e-posta olsun
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

            // ID Identity tarafından atanır

            // LoginDto -> Admin (Sadece mapleme için, Identity doğrulaması için kullanılmaz)
            //CreateMap<LoginDto, AdminUser>()
            //    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            //    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));
        }
    }
}
