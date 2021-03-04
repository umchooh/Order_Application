using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Order_Application.Models.ViewModel
{
    public class ShowOrder
    {
        public OrderDto Order { get; set; }

        public IEnumerable<ProductDto> Products { get; set; }

        public IEnumerable<OrderItemDto> OrderItems { get; set; }
    }
}