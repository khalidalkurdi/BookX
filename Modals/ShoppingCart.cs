using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MyProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("user")]
        [ValidateNever]
        public string UserID { get; set; }
        [ForeignKey("product")]
        [ValidateNever]
        public int ProductId { get; set; }
        [Range(1,1000,ErrorMessage ="please enter a value between 1 and 1000")]
        public int Count { get; set; }

        public Product product { get; set; }
        public ApplicationUser user { get; set; }

        [NotMapped]
        public double CartPrice { get; set; }

    }
}
