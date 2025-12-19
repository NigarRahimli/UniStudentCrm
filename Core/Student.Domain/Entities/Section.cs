using Student.Domain.Entities.Common;


namespace Student.Domain.Entities
{
    public class Section:BaseEntity
    {
        //// Section = a specific offering of a Course in a Term (semester).
        // Example: CS101 in Fall 2025 can have Section A (Teacher1) and Section B (Teacher2).
        // Students enroll into a Section (the real class: teacher/schedule/group), not just the Course.

        public string SectionCode { get; set; } = null!; // A, B, 01...

        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public Guid TermId { get; set; }
        public Term Term { get; set; } = null!;

        public Guid TeacherId { get; set; }
        public TeacherUser Teacher { get; set; } = null!;

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    }
}
