namespace StudentCrm.Application.DTOs.Student
{
    public class StudentDto
    {
        public string Id { get; set; }
        public string StudentNo { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool HasLogin { get; set; }  // true if AppUserId is set
    }
}
