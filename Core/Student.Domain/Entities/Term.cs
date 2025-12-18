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
        public string Name { get; set; } // 2025 Fall
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
