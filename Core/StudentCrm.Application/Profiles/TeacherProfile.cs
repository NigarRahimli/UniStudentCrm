using AutoMapper;
using Student.Domain.Entities;
using StudentCrm.Application.DTOs.Teacher;
using StudentCrm.Application.DTOs.Section;

namespace StudentCrm.Application.Profiles
{
    public class TeacherProfile : Profile
    {
        public TeacherProfile()
        {
 
            CreateMap<CreateTeacherDto, TeacherUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())          // ignore ID, it is generated
                .ForMember(dest => dest.AppUser, opt => opt.Ignore())      // service handles identity creation
                .ForMember(dest => dest.AppUserId, opt => opt.Ignore())    // set after AppUser created
                .ForMember(dest => dest.Sections, opt => opt.Ignore());    // service links sections

            CreateMap<UpdateTeacherDto, TeacherUser>()
                .ForMember(dest => dest.AppUser, opt => opt.Ignore())
                .ForMember(dest => dest.AppUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Sections, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition(
                     (src, dest, srcMember) => srcMember != null));        // skip nulls

     
            CreateMap<TeacherUser, TeacherDto>()
                .ForMember(d => d.Sections, opt => opt.MapFrom(s => s.Sections))
                .ForMember(d => d.AppUserId, opt => opt.MapFrom(src => src.AppUserId));

         
         
        }
    }
}
