﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class OrderVM
    {  
        public OrderHeader orderHeader { get; set; }
        public IEnumerable<OrderDetail>  orderDetail { get; set; }
    }
}
