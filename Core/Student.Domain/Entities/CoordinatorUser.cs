using Student.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student.Domain.Entities
{
    public class CoordinatorUser: BaseEntity
    {
        public string FullName { get; set; } = null!;
        public string? Department { get; set; }
        public int CoordinatorNo { get; set; }

        public Guid AppUserId { get; set; }       // coordinator login olur
        public AppUser AppUser { get; set; } = null!;
    }
}
