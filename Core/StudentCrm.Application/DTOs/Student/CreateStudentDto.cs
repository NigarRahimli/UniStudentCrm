namespace StudentCrm.Application.DTOs.Student
{
    public class CreateStudentDto
    {
        public string StudentNo { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string Major { get; set; }
        public double? GPA { get; set; }
    }
}
