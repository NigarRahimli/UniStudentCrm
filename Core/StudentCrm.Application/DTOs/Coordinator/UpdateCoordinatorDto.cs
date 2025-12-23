using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Application.DTOs.Coordinator
{
    public class UpdateCoordinatorDto
    {
        public string Id { get; set; }
        public string? FullName { get; set; }
        public string? Department { get; set; }
        public int? CoordinatorNo { get; set; }

        // login email update üçün
        public string? Email { get; set; }
    }
}
