using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models
{
    public class CategoryWithNumberOfJobs
    {
        public int Id { get; set; }
        public string? CategoryName { get; set; }
        public int JobQuanities { get; set; }
    }
}
