using Student.Domain.Entities.Common;


namespace Student.Domain.Entities
{
    public class Enrollment:BaseEntity
    {

        public int StudentId { get; set; }
        public UniStudent Student { get; set; }

        public int SectionId { get; set; }
        public Section Section { get; set; }

        // sadə qiymətləndirmə
        public decimal? TotalGrade { get; set; }  // 0-100
        public string LetterGrade { get; set; }   // A,B,C...
    }
}
