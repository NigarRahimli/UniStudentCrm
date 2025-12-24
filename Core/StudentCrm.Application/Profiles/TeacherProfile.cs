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
            // CreateTeacherDto -> TeacherUser
            CreateMap<CreateTeacherDto, TeacherUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AppUser, opt => opt.Ignore())
                .ForMember(dest => dest.AppUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Sections, opt => opt.Ignore());

            // UpdateTeacherDto -> TeacherUser (PATCH behavior)
            CreateMap<UpdateTeacherDto, TeacherUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // id entity-dən tapılır
                .ForMember(dest => dest.AppUser, opt => opt.Ignore())
                .ForMember(dest => dest.AppUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Sections, opt => opt.Ignore())
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) =>
                        srcMember != null &&
                        !(srcMember is string s && string.IsNullOrWhiteSpace(s))
                    )
                );

            // TeacherUser -> TeacherDto
            CreateMap<TeacherUser, TeacherDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.AppUserId, opt => opt.MapFrom(s => s.AppUserId.ToString()))
                .ForMember(d => d.Sections, opt => opt.MapFrom(s => s.Sections));

            CreateMap<TeacherUser, TeacherDto>()
               .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()));
            CreateMap<TeacherUser, TeacherShortDto>()
               .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()));

        }
    }
}
