using AutoMapper;
using Student.Domain.Entities;
using StudentCrm.Application.DTOs.Student;
using StudentCrm.Application.DTOs.Enrollment;

namespace StudentCrm.Application.Profiles
{
    public class StudentProfile : Profile
    {
        public StudentProfile()
        {
            // Entity -> StudentDto
            CreateMap<StudentUser, StudentDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.HasLogin, opt => opt.MapFrom(s => s.AppUserId != null));

            // Entity -> StudentDetailDto
            CreateMap<StudentUser, StudentDetailDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id)) // StudentDetailDto.Id Guid-dir
                .ForMember(d => d.HasLogin, opt => opt.MapFrom(s => s.AppUserId != null))
                // Enrollments map olunacaq (EnrollmentProfile varsa daha yaxşı)
                .ForMember(d => d.Enrollments, opt => opt.MapFrom(s => s.Enrollments));

            // CreateStudentDto -> Entity
            CreateMap<CreateStudentDto, StudentUser>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.AppUserId, opt => opt.Ignore())
                .ForMember(d => d.AppUser, opt => opt.Ignore())
                .ForMember(d => d.Enrollments, opt => opt.Ignore());

            // UpdateStudentDto -> Entity (partial update)
            // NOTE: UpdateStudentDto.Id stringdir, entity Id Guiddir => ignore edirik
            CreateMap<UpdateStudentDto, StudentUser>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.AppUserId, opt => opt.Ignore())
                .ForMember(d => d.AppUser, opt => opt.Ignore())
                .ForMember(d => d.Enrollments, opt => opt.Ignore())
                // yalnız null olmayanları update et
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
