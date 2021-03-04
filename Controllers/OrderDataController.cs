using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Diagnostics;
using Order_Application.Models;

namespace Order_Application.Controllers
{
    ///Reference for the project obtained from https://github.com/christinebittle/varsity_mvp.git/commit/8467279ed35c72dbac3d1cb86349301aa89d159a , Feb 2021 accessed. 
    public class OrderDataController : ApiController
    {
        //The Database Access Point
        private PassionDbContext db = new PassionDbContext();

        // This api contorller form form WebAPI2 with read/write action with EF.

        /// <summary>
        /// Gets a list of Order in the database alongside a status code (200 OK)
        /// </summary>
        /// <returns>A list of Orders including their ID, Date and Subtotal</returns>
        ///<example> GET: api/OrderData/GetOrders </example>
        [ResponseType(typeof(IEnumerable<OrderDto>))]
        public IEnumerable<OrderDto> GetOrders()
        {
            List<Order> Orders = db.Orders.ToList();
            List<OrderDto> OrderDtos = new List<OrderDto> { };

            //Here you can choose which information is exposed to the API
            foreach (var Order in Orders)
            {
                OrderDto NewOrder = new OrderDto
                {
                    OrderID = Order.OrderID,
                    OrderDate = Order.OrderDate,
                    CustomerID = Order.CustomerID
                };
                OrderDtos.Add(NewOrder);
            }

            return (OrderDtos);
        }

        
        /// <summary>
        /// Finds a particular Order in the database given a Customer id with a 200 status code. If the order is not found, return 404.
        /// </summary>
        /// <param name="id">The Customer id</param>
        /// <returns>Information about the order, including Customer id, Order Date and Order ID</returns>
        // <example>
        // GET: api/OrderData/FindCustomerForOrder/5
        // </example>
        [HttpGet]
        [ResponseType(typeof(CustomerDto))]
        public IHttpActionResult GetOrdersForCustomer(int id)
        {
            //Finds the Orders on particular customerID; return as a list form that show customer ID , Order Date and Order ID.
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }

            ICollection<Order> orders = customer.Orders;

            List<OrderDto> orderDTOs = new List<OrderDto>();
            foreach (var order in orders)
            {
                OrderDto NewOrder = new OrderDto
                {
                    OrderID = order.OrderID,
                    OrderDate = order.OrderDate,
                    CustomerID = customer.CustomerID
                };
                orderDTOs.Add(NewOrder);
            }


            //pass along data as 200 status code OK response
            return Ok(orderDTOs);
        }

        ///This method need more steps, to modifiy to connect thru OrderItems bridging Table to run create and update functionality for OrderController !
        /*// <summary>
        /// Gets a list or items in the OrderItems Databse
        /// </summary>
        /// <param name="id">The input orderid</param>
        /// <returns>A list of Productss including their ID, name, Description and price.</returns>
        /// <example>
        /// GET: api/ProductData/GetProductsForOrder
        /// </example>
        [ResponseType(typeof(IEnumerable<ProductDto>))]
        public IHttpActionResult GetProductsForOrder(int id)
        {
            List<Product> Products = db.Products
                .Where(p =>p.Orders.Any(o => o.OrderID == id))
                .ToList();
            List<ProductDto> ProductDtos = new List<ProductDto> { };

            //Here you can choose which information is exposed to the API
            foreach (var Product in Products)
            {
                ProductDto NewProduct = new ProductDto
                {
                    ProductID = Product.ProductID,
                    ProductName = Product.ProductName,
                    ProductDescription = Product.ProductDescription,
                    ProductPrice = Product.ProductPrice
                                   
                };
                ProductDtos.Add(NewProduct);
            }

            return Ok(ProductDtos);
        }*/

        ///This method will need to modify to work with OrderItems to set up the form list of items can be placed.
        /// <summary>
        /// Add a Order with a litd of items, and qubtity , with fix unit price ,  to the database after receive the GET form information
        /// </summary>
        /// <param name="order">An order object. Sent as POST form data.</param>
        /// <returns>If request is OK indicated, it is working! if failed, it will return error message of 400 status code</returns>
        /// <example>
        /// POST: api/OrderData/AddOrder
        ///  FORM DATA: Order JSON Object
        /// </example>
        [ResponseType(typeof(Order))]
        [HttpPost]
        public IHttpActionResult AddOrder([FromBody] Order order)
        {
            //Will Validate according to data annotations specified on model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Orders.Add(order);
            db.SaveChanges();

            return Ok(order.OrderID);
        }

        /// <summary>
        /// Finds a particular Order in the database with a 200 status code. If the Team is not found, return 404.
        /// </summary>
        /// <param name="id">The Order id</param>
        /// <returns>Information about the Order, including Order id, bio, first and last name, and teamid</returns>
        // <example>
        // GET: api/OrderData/FindOrder/5
        // </example>
        [HttpGet]
        [ResponseType(typeof(OrderDto))]
        public IHttpActionResult FindOrder(int id)
        {
            //Find the data
            Order Order = db.Orders.Find(id);
            //if not found, return 404 status code.
            if (Order == null)
            {
                return NotFound();
            }

            //put into a 'friendly object format'
            OrderDto OrderDto = new OrderDto
            {
                OrderID = Order.OrderID,
                CustomerID = Order.CustomerID,
                OrderDate = Order.OrderDate
               
            };


            //pass along data as 200 status code OK response
            return Ok(OrderDto);
        }


        ///This method need to modify more in future. to corporate along with orderItems bridging table, to carry out Create and Update functionality
        /// <summary>
        /// Updates a Order information after any changes apply.
        /// </summary>
        /// <param name="id">The order id</param>
        /// <param name="Order">A order object. Received as POST data.</param>
        /// <returns></returns>
        /// <example>
        /// POST: api/OrderData/UpdateOrder5
        /// FORM DATA: Order JSON Object
        /// </example>
        [ResponseType(typeof(void))]
        [HttpPost]
        public IHttpActionResult UpdateOrder(int id, [FromBody] Order Order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != Order.OrderID)
            {
                return BadRequest();
            }

            db.Entry(Order).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }
        /// <summary>
        /// Receive Get information from client-side and deletes a order in the database
        /// </summary>
        /// <param name="id">The id of the order to delete.</param>
        /// <returns>If failed to delete , it will show error page. If succeed , it will show save the changes and confimed request of 200 status code </returns>
        /// <example>
        /// POST: api/OrderData/DeleteOrder/5
        /// </example>
        [HttpPost]
        // DELETE: api/OrderData/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult DeleteOrder(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            db.Orders.Remove(order);
            db.SaveChanges();

            return Ok(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        /// <summary>
        /// Finds an order in the system. Internal use only.
        /// </summary>
        /// <param name="id">The order id</param>
        /// <returns>TRUE if the order exists, false otherwise.</returns>
        private bool OrderExists(int id)
        {
            return db.Orders.Count(e => e.OrderID == id) > 0;
        }
    }
}