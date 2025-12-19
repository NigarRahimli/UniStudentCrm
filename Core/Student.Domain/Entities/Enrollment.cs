using Student.Domain.Entities.Common;


namespace Student.Domain.Entities
{
    public class Enrollment:BaseEntity
    {

        public Guid StudentId { get; set; }
        public StudentUser Student { get; set; } = null!;

        public Guid SectionId { get; set; }
        public Section Section { get; set; } = null!;

        public decimal? TotalGrade { get; set; }  // 0-100
        public string? LetterGrade { get; set; }  // A,B,C...
    }
}

