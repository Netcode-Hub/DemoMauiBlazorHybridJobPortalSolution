using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models
{
    public class GetJob
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Function { get; set; }
        public string? Description { get; set; }
        public decimal MinSalaryRange { get; set; }
        public decimal MaxSalaryRange { get; set; }
        public string? JobMode { get; set; }
        public string? JobLocation { get; set; }
        public bool Active { get; set; } = true;
        public bool Featured { get; set; } = false;
        public string? CompanyEmail { get; set; }
        public string? Phone { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyLocation { get; set; }
        public string? CompanyLogo { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
