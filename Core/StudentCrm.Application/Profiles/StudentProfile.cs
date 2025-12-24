using AutoMapper;
using Student.Domain.Entities;
using StudentCrm.Application.DTOs.Student;

namespace StudentCrm.Application.Profiles
{
    public class StudentProfile : Profile
    {
        public StudentProfile()
        {
            // Entity -> StudentDto
            CreateMap<StudentUser, StudentDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.AppUser != null ? s.AppUser.Email : null));

            // Entity -> StudentDetailDto
            CreateMap<StudentUser, StudentDetailDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.AppUser != null ? s.AppUser.Email : null))
                .ForMember(d => d.Enrollments, opt => opt.MapFrom(s => s.Enrollments));

            // CreateStudentDto -> Entity
            // AppUser, AppUserId service-də set olunacaq
            CreateMap<CreateStudentDto, StudentUser>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.AppUserId, opt => opt.Ignore())
                .ForMember(d => d.AppUser, opt => opt.Ignore())
                .ForMember(d => d.Enrollments, opt => opt.Ignore())
                // CreateStudentDto.Email AppUser yaratmaq üçün istifadə olunacaq, StudentUser-a map etmə
                .ForSourceMember(s => s.Email, opt => opt.DoNotValidate());

            // UpdateStudentDto -> Entity (partial update)
            CreateMap<UpdateStudentDto, StudentUser>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.AppUserId, opt => opt.Ignore())
                .ForMember(d => d.AppUser, opt => opt.Ignore())
                .ForMember(d => d.Enrollments, opt => opt.Ignore())
                // UpdateStudentDto.Email AppUser-a aiddir, StudentUser-a map etmə
                .ForSourceMember(s => s.Email, opt => opt.DoNotValidate())
                // yalnız null olmayanları update et
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
