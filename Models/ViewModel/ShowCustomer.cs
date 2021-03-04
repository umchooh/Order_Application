using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Order_Application.Models.ViewModel
{
    public class ShowCustomer
    {
        public CustomerDto customer { get; set; }


        IEnumerable<OrderDto> orders { get; set; }
    }
}