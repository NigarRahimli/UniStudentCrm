using AutoMapper;
using Student.Domain.Entities;
using StudentCrm.Application.DTOs.Enrollment;
using StudentCrm.Application.DTOs.Student;
using StudentCrm.Application.DTOs.Section;

namespace StudentCrm.Application.
    s
{
    public class EnrollmentProfile : Profile
    {
        public EnrollmentProfile()
        {
           
            CreateMap<CreateEnrollmentDto, Enrollment>()
                .ForMember(d => d.Id, o => o.Ignore())     // DB/EF sets Id
                .ForMember(d => d.Student, o => o.Ignore()) // We use just StudentId
                .ForMember(d => d.Section, o => o.Ignore());

        
            CreateMap<UpdateEnrollmentDto, Enrollment>()
                .ForMember(d => d.Student, o => o.Ignore())
                .ForMember(d => d.Section, o => o.Ignore());

      
            CreateMap<Enrollment, EnrollmentDto>()
                .ForMember(d => d.Student, o => o.MapFrom(s => s.Student))
                .ForMember(d => d.Section, o => o.MapFrom(s => s.Section));
        }
    }
}
