using AutoMapper;
using Student.Domain.Entities;
using StudentCrm.Application.DTOs.Coordinator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.Profiles
{
    public class CoordinatorProfile : Profile
    {
        public CoordinatorProfile()
        {
    
            CreateMap<CreateCoordinatorDto, CoordinatorUser>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.AppUserId, o => o.Ignore()) // service set edəcək (user yaradandan sonra)
                .ForMember(d => d.AppUser, o => o.Ignore());

           
            CreateMap<UpdateCoordinatorDto, CoordinatorUser>()
                .ForMember(d => d.AppUserId, o => o.Ignore())
                .ForMember(d => d.AppUser, o => o.Ignore())
                .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CoordinatorUser, CoordinatorDto>()
                .ForMember(d => d.Email, o => o.MapFrom(s => s.AppUser.Email));
        }
    }
}
