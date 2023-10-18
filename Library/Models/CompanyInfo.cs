using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models
{
    public class CompanyInfo
    {
        public int Id { get; set; }
        [Required, MinLength(5), MaxLength(100)]
        public string? CompanyName { get; set; }
        [Required, MinLength(5), MaxLength(1000), DataType(DataType.MultilineText)]
        public string? CompanyAddress { get; set; }
        [Required, MinLength(5), MaxLength(1000), DataType(DataType.MultilineText)]
        public string? CompanyLocation { get; set; }
        [Required]
        public string? CompanyLogo { get; set; } = string.Empty;
        [Required]
        public string? CompanyCertificateName { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public bool Granted { get; set; } = false;
    }
}
