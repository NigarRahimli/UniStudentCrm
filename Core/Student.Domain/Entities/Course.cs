using Student.Domain.Entities.Common;

namespace Student.Domain.Entities
{
    public class Course:BaseEntity
    {
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int Credit { get; set; }

        public ICollection<Section> Sections { get; set; } = new List<Section>();
    }
}
