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
    public class CustomerDataController : ApiController
    {
        //The Database Access Point
        private PassionDbContext db = new PassionDbContext();

        // This api contorller form form WebAPI2 with read/write action with EF.

        /// <summary>
        /// Retrieve all customers information
        /// </summary>
        /// <returns>A list of customers with their information on FirstName, LastName, PhoneNumber, Email and Shipping Address</returns>
        ///<example> GET: api/CustomerData/GetCustomers </example>
        [ResponseType(typeof(IEnumerable<CustomerDto>))]
        public IEnumerable<CustomerDto> GetCustomers()
        {
            List<Customer> Customers = db.Customers.ToList();
            List<CustomerDto> CustomerDtos = new List<CustomerDto> { };

            //Here you can choose which information is exposed to the API
            foreach (var Customer in Customers)
            {
                CustomerDto NewCustomer = new CustomerDto
                {
                    CustomerID = Customer.CustomerID,
                    CustomerFirstName = Customer.CustomerFirstName,
                    CustomerLastName = Customer.CustomerLastName,
                    CustomerPhoneNum = Customer.CustomerPhoneNum,
                    CustomerEmail = Customer.CustomerEmail,
                    CustomerShipping = Customer.CustomerShipping,

                };
                CustomerDtos.Add(NewCustomer);
            }

            return (CustomerDtos);
        }

        /// <summary>
        /// Finds a particular customer in the database with a 200 status code. If the customer is not found, return 404.
        /// </summary>
        /// <param name="id">The customer id</param>
        /// <returns>Information about the particular customer includes FirstName, LastName, PhoneNumber, Email and Shipping Address  </returns>
        // <example>
        // GET: api/CustomerData/FindCustomer/5
        // </example>
        [HttpGet]
        [ResponseType(typeof(CustomerDto))]
        public IHttpActionResult FindCustomer(int id)
        {
            //Find a customer Data
            Customer Customer = db.Customers.Find(id);
            //If not found, return 404 status code
            if (Customer == null)
            {
                return NotFound();
            }

            //put into a 'friendly object format'
            CustomerDto CustomerDto = new CustomerDto
            {
                CustomerID = Customer.CustomerID,
                CustomerFirstName = Customer.CustomerFirstName,
                CustomerLastName = Customer.CustomerLastName,
                CustomerPhoneNum = Customer.CustomerPhoneNum,
                CustomerEmail = Customer.CustomerEmail,
                CustomerShipping = Customer.CustomerShipping,


            };

            //pass along data as 200 status code OK response
            return Ok(CustomerDto);
        }

        /// <summary>
        /// Updates a Customer information after any changes apply.
        /// </summary>
        /// <param name="id">The Customer id</param>
        /// <param name="Customer">A Customer object. Received as POST data.</param>
        /// <returns>Updated Customer infromation on Database</returns>
        /// <example>
        /// POST: api/CustomerData/UpdateCustomer/5
        /// FORM DATA: Customer JSON Object
        /// </example>
        [ResponseType(typeof(void))]
        [HttpPost]
        public IHttpActionResult UpdateCustomer(int id, [FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != customer.CustomerID)
            {
                return BadRequest();
            }

            db.Entry(customer).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
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
        /// Add a Customer to the database after receive the GET form information
        /// </summary>
        /// <param name="customer">A Customer object. Sent as POST form data.</param>
        /// <returns>If request is OK indicated, it is working! if failed, it will return error message of 400 status code</returns>
        /// <example>
        /// POST: api/CustomerData/AddCustomer
        ///  FORM DATA: Customer JSON Object
        /// </example>
        [ResponseType(typeof(Customer))]
        [HttpPost]
        public IHttpActionResult AddCustomer([FromBody] Customer customer)
        {
            //Will Validate according to data annotations specified on model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Customers.Add(customer);
            db.SaveChanges();

            return Ok(customer.CustomerID);
        }

        /// <summary>
        /// Receive Get information from client-side and deletes a customer in the database
        /// </summary>
        /// <param name="id">The id of the customer to delete.</param>
        /// <returns>If failed to delete , it will show error page. If succeed , it will show save the changes and confimed request of 200 status code </returns>
        /// <example>
        /// POST: api/CustomerData/DeleteCustomer/5
        /// </example>
        [HttpPost]
        public IHttpActionResult DeleteCustomer(int id)
        {
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }

            db.Customers.Remove(customer);
            db.SaveChanges();

            return Ok();
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
        /// Finds a customer in the system. Internal use only.
        /// </summary>
        /// <param name="id">The customer id</param>
        /// <returns>TRUE if the customer exists, false otherwise.</returns>
        private bool CustomerExists(int id)
        {
            return db.Customers.Count(e => e.CustomerID == id) > 0;
        }
    }
}