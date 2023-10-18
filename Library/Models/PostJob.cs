using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models
{
    public class PostJob
    {
        public int Id { get; set; }
        [Required, MinLength(5), MaxLength(200), DataType(DataType.Text)]
        public string? Title { get; set; }
        [Required, MinLength(5), MaxLength(100), DataType(DataType.Text)]
        public string? Function { get; set; }
        [Required, MinLength(5), MaxLength(1000), DataType(DataType.MultilineText)]
        public string? Description { get; set; }
        [DataType(DataType.Currency), Range(0.00, 999999.00)]
        public decimal MinSalaryRange { get; set; } = 0.00m;
        [DataType(DataType.Currency), Range(0.00, 999999.00)]
        public decimal MaxSalaryRange { get; set; } = 0.00m;
        [Required]
        public string? JobMode { get; set; }
        [Required, MinLength(5), MaxLength(1000), DataType(DataType.MultilineText)]
        public string? JobLocation { get; set; }
        public bool Active { get; set; } = true;
        public bool Featured { get; set; } = false;
        public int ClientId { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;

        // Relationship
        public Category? Category { get; set; }
        [Required]
        public int CategoryId { get; set; }
    }
}
