using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Order_Application.Models.ViewModel
{
    public class UpdateOrder
    {
        public CustomerDto customer { get; set; }

        public OrderDto order { get; set; }


        IEnumerable<OrderDto> orders { get; set; }
    }
}