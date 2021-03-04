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
using Order_Application.Models;

namespace Order_Application.Controllers
{
    public class OrderItemsDataController : ApiController
    {
        private PassionDbContext db = new PassionDbContext();

        // Get: api/FindItemsOfOrder/5
        [HttpGet]
        public IQueryable<OrderItems> FindItemsOfOrder(int id)
        {
            return db.OrderItems.Where(orderItem => orderItem.OrderID == id);
        }

        // GET: api/OrderItemsData
        public IQueryable<OrderItems> GetOrderItems()
        {
            return db.OrderItems;
        }

        // GET: api/OrderItemsData/5
        [ResponseType(typeof(OrderItems))]
        public IHttpActionResult GetOrderItems(int id)
        {
            OrderItems orderItems = db.OrderItems.Find(id);
            if (orderItems == null)
            {
                return NotFound();
            }

            return Ok(orderItems);
        }

        // PUT: api/OrderItemsData/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutOrderItems(int id, OrderItems orderItems)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != orderItems.OrderID)
            {
                return BadRequest();
            }

            db.Entry(orderItems).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderItemsExists(id))
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

        // POST: api/OrderItemsData
        [ResponseType(typeof(OrderItems))]
        public IHttpActionResult PostOrderItems(OrderItems orderItems)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.OrderItems.Add(orderItems);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (OrderItemsExists(orderItems.OrderID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = orderItems.OrderID }, orderItems);
        }

        // DELETE: api/OrderItemsData/5
        [ResponseType(typeof(OrderItems))]
        public IHttpActionResult DeleteOrderItems(int id)
        {
            OrderItems orderItems = db.OrderItems.Find(id);
            if (orderItems == null)
            {
                return NotFound();
            }

            db.OrderItems.Remove(orderItems);
            db.SaveChanges();

            return Ok(orderItems);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrderItemsExists(int id)
        {
            return db.OrderItems.Count(e => e.OrderID == id) > 0;
        }
    }
}