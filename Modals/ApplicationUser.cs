


using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{       
    public class ApplicationUser : IdentityUser
    {
        [ForeignKey("company")]
        public int? CompanyId { get; set; }
        [Required]
        public string Name { get; set; }
        public string? City { get; set; }
        public string? StreetAddress { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        [ValidateNever]       
        public Company company { get; set; }
    }
}
