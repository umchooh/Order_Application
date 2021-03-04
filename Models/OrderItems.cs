using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Order_Application.Models
{
    public class OrderItems
    {
        public int OrderID { get; set; }

        public double Amount { get; set; }
        public decimal ProductPrice { get; set; }
        public int ProductID { get; set; }
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
    
    public class OrderItemDto
    {
        public ProductDto Product { get; set; }
        public double ProductPrice { get; set; }
        public decimal Amount { get; set; }
    }
}