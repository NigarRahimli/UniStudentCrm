using Student.Domain.Entities.Common;


namespace Student.Domain.Entities
{
    public class UniStudent:BaseEntity
    {
        public string StudentNo { get; set; }  
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

}
