using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace Order_Application.Models
{
    public class Customer
    {

        [Key]
        public int CustomerID { get; set; }

        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerPhoneNum { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerShipping { get; set; }

        //An customer can place many orders
        public ICollection<Order> Orders { get; set; }

    }

    public class CustomerDto
    {
        public int CustomerID { get; set; }
        [DisplayName("First Name")]
        public string CustomerFirstName { get; set; }
        [DisplayName("Last Name")]
        public string CustomerLastName { get; set; }
        [DisplayName("Phone Number")]
        public string CustomerPhoneNum { get; set; }
        [DisplayName("Email Address")]
        public string CustomerEmail { get; set; }
        [DisplayName("Shipping Address")]
        public string CustomerShipping { get; set; }
    }
}