using Student.Domain.Entities.Common;


namespace Student.Domain.Entities
{
    public class Section:BaseEntity
    {
        //// Section = a specific offering of a Course in a Term (semester).
        // Example: CS101 in Fall 2025 can have Section A (Teacher1) and Section B (Teacher2).
        // Students enroll into a Section (the real class: teacher/schedule/group), not just the Course.

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public int TermId { get; set; }
        public Term Term { get; set; }

        public string SectionCode { get; set; } // A, B, 01

        public int TeacherId { get; set; }
        public  Teacher Teacher{ get; set; }

    }
}
