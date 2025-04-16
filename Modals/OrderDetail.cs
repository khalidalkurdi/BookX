using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MyProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        [ForeignKey(nameof(orderHeader))]
        public int OrderHeaderId { get; set; }
        [ValidateNever]
        public OrderHeader orderHeader { get; set; }

        [ForeignKey(nameof(product))]
        public int ProductId { get; set; }
        [ValidateNever]
        public Product product { get; set; }
        public int Count { get; set; }
        public double Price { get; set; }

    }
}
