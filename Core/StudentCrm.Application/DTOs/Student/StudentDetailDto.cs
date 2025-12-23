using StudentCrm.Application.DTOs.Enrollment;

namespace StudentCrm.Application.DTOs.Student
{
    public class StudentDetailDto
    {
        public Guid Id { get; set; }
        public string StudentNo { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool HasLogin { get; set; }

        public List<EnrollmentDto> Enrollments { get; set; } = new();
    }
}
