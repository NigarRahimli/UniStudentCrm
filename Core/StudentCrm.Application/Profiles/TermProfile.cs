using AutoMapper;
using Student.Domain.Entities;
using StudentCrm.Application.DTOs.Term;
using StudentCrm.Application.DTOs.Section;

namespace StudentCrm.Application.Profiles
{
    public class TermProfile : Profile
    {
        public TermProfile()
        {
            CreateMap<CreateTermDto, Term>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Sections, opt => opt.Ignore());
            
            CreateMap<UpdateTermDto, Term>()
                .ForMember(d => d.Sections, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition(
                    (src, dest, srcMember) => srcMember != null));


            CreateMap<Term, TermDto>()
                .ForMember(d => d.Sections, opt => opt.MapFrom(s => s.Sections));

    
          
        }
    }
}
