using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Order_Application.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        public DateTime OrderDate { get; set; }

        //An order has one customer
        [ForeignKey("Customer")]
        public int CustomerID { get; set; }

        public virtual Customer Customer { get; set; }
        public ICollection<OrderItems> OrderItems { get; set; }
    }

    //This class can be used totransfer information about an order
    //also known as a "Data Transfer Object"
    //vessel communication "Model-liked"
    public class OrderDto
    {
        public int OrderID { get; set; }
        [DisplayName("Date")]
        public DateTime OrderDate { get; set; }

        [DisplayName("CustomerID")]
        public int CustomerID { get; set; }

        [DisplayName("Customer")]
        public CustomerDto Customer { get; set; }
        
    }
}