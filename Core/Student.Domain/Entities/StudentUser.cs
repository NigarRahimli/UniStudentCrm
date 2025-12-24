using Student.Domain.Entities.Common;


namespace Student.Domain.Entities
{
    public class StudentUser:BaseEntity
    {
        public string StudentNo { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Email { get; set; }

        // Hamı login olacaqsa -> Guid AppUserId (required) et
        public Guid AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public string Major { get; set; }
        public double? GPA { get; set; }

    }

}
