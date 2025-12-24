using AutoMapper;
using Student.Domain.Entities;
using StudentCrm.Application.DTOs.Section;

namespace StudentCrm.Application.Profiles
{
    public class SectionProfile : Profile
    {
        public SectionProfile()
        {
            // Create: DTO -> Entity
            CreateMap<CreateSectionDto, Section>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Enrollments, opt => opt.Ignore()); // handled separately

            // Update: DTO -> Entity
            CreateMap<UpdateSectionDto, Section>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Enrollments, opt => opt.Ignore()); // handled by Enrollment service

            // Entity -> DTO (for GET)
            CreateMap<Section, SectionDto>();
            CreateMap<Section, SectionShortDto>();
            CreateMap<Section, SectionTeacherDto>();
        }
    }
}
