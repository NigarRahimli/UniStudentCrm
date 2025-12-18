using Student.Domain.Entities.Common;

namespace Student.Domain.Entities
{
    public class Course:BaseEntity
    {
        public string Code { get; set; }   // CS101
        public string Title { get; set; }  // OOP
        public int Credit { get; set; }
    }
}
