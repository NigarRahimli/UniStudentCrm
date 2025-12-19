using Student.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student.Domain.Entities
{
    public class Term:BaseEntity
    {
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<Section> Sections { get; set; } = new List<Section>();
    }
}
