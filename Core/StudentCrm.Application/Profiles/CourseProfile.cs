using AutoMapper;
using Student.Domain.Entities;
using StudentCrm.Application.DTOs.Course;
using StudentCrm.Application.DTOs.Section;

namespace StudentCrm.Application.Profiles
{
    public class CourseProfile : Profile
    {
        public CourseProfile()
        {
            CreateMap<CreateCourseDto, Course>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Sections, opt => opt.Ignore());

            CreateMap<UpdateCourseDto, Course>()
                .ForMember(dest => dest.Sections, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition(
                    (src, dest, srcMember) => srcMember != null));

            CreateMap<Course, CourseDto>();
            CreateMap<Course, CourseShortDto>();
           
        }
    }

}
