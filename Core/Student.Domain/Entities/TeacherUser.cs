using Student.Domain.Entities.Common;


namespace Student.Domain.Entities
{
    public class TeacherUser: BaseEntity
    {
        public int StaffNo { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }

        public Guid AppUserId { get; set; }       // müəllim mütləq login olur adətən
        public AppUser AppUser { get; set; } = null!;

        public ICollection<Section>? Sections { get; set; } = new List<Section>();

    }
}
