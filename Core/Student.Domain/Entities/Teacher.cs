using Student.Domain.Entities.Common;


namespace Student.Domain.Entities
{
    public class Teacher:BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int StaffNo { get; set; }
    }
}
