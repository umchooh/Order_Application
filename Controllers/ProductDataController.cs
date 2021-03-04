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
using System.Web;
using System.IO;

namespace Order_Application.Controllers
{

    ///Reference for the project obtained from https://github.com/christinebittle/varsity_mvp.git/commit/8467279ed35c72dbac3d1cb86349301aa89d159a , Feb 2021 accessed. 
    public class ProductDataController : ApiController
    {
        //The Database Access Point
        private PassionDbContext db = new PassionDbContext();

        // This api contorller form form WebAPI2 with read/write action with EF.

        /// <summary>
        /// Gets a list of products in the database alongside a status code (200 OK)
        /// </summary>
        /// <returns>A list of  products with their Information about the product, Name, description, price, hasPic, and Pic Extension</returns>
        ///<example> GET: api/ProductData/GetProducts </example>
        [ResponseType(typeof(IEnumerable<ProductDto>))]
        public IEnumerable<ProductDto> GetProducts()
        {
            List<Product> Products = db.Products.ToList();
            List<ProductDto> ProductDtos = new List<ProductDto> { };

            //Here you can choose which information is exposed to the API
            foreach (var Product in Products)
            {
                ProductDto NewProduct = new ProductDto
                {
                    ProductID = Product.ProductID,
                    ProductName = Product.ProductName,
                    ProductDescription = Product.ProductDescription,
                    ProductPrice = Product.ProductPrice,
                    ProductHasPic = Product.ProductHasPic,
                    PicExtension = Product.PicExtension

                };
                ProductDtos.Add(NewProduct);
            }

            return (ProductDtos);

        }

        /// <summary>
        /// Gets a list or players in the database alongside a status code (200 OK). Skips the first {startindex} records and takes {perpage} records.
        /// </summary>
        /// <returns>A list of products including their ID, name description, price and picture.</returns>
        /// <param name="StartIndex">The number of records to skip through</param>
        /// <param name="PerPage">The number of records for each page</param>
        /// <example>
        /// GET: api/productData/GetProducts/20/5
        /// Retrieves the first 5 products after skipping 20 (fifth page)
        /// 
        /// GET: api/productData/Getproducts/15/3
        /// Retrieves the first 3 products after skipping 15 (sixth page)
        /// 
        /// </example>
        [ResponseType(typeof(IEnumerable<ProductDto>))]
        [Route("api/productdata/getproductspage/{StartIndex}/{PerPage}")]
        public IHttpActionResult GetProductsPage(int StartIndex, int PerPage)
        {
            List<Product> Products = db.Products.OrderBy(p => p.ProductID).Skip(StartIndex).Take(PerPage).ToList();
            List<ProductDto> ProductDtos = new List<ProductDto> { };

            //Here you can choose which information is exposed to the API
            foreach (var Product in Products)
            {
                ProductDto NewProduct = new ProductDto
                {
                    ProductID = Product.ProductID,
                    ProductName = Product.ProductName,
                    ProductDescription = Product.ProductDescription,
                    ProductPrice = Product.ProductPrice,
                    ProductHasPic = Product.ProductHasPic,
                    PicExtension = Product.PicExtension
                };
                ProductDtos.Add(NewProduct);
            }

            return Ok(ProductDtos);
        }


        /// <summary>
        /// Finds a particular Product in the database with a 200 status code. If the customer is not found, return 404.
        /// </summary>
        /// <param name="id">The product id</param>
        /// <returns>Information about the slsected product information including, Name, description, price, hasPic, and Pic Extension</returns>
        // <example>
        // GET: api/ProductData/FindProduct/5
        // </example>
        [HttpGet]
        [ResponseType(typeof(ProductDto))]
        public IHttpActionResult FindProduct(int id)
        {
            //Find a product Data
            Product Product = db.Products.Find(id);
            //If not found, return 404 status code
            if (Product == null)
            {
                return NotFound();
            }

            //put into a 'friendly object format'
            ProductDto ProductDto = new ProductDto
            {
                ProductID = Product.ProductID,
                ProductName = Product.ProductName,
                ProductDescription = Product.ProductDescription,
                ProductPrice = Product.ProductPrice,
                ProductHasPic = Product.ProductHasPic,
                PicExtension = Product.PicExtension
            };

            //pass along data as 200 status code OK response
            return Ok(ProductDto);
        }

       
        /// <summary>
        /// Updates product information after any changes apply.
        /// </summary>
        /// <param name="id">The product id</param>
        /// <param name="Product">A product object. Received as POST data.</param>
        /// <returns></returns>
        /// <example>
        /// POST: api/ProductData/UpdateProduct/5
        /// FORM DATA: Product JSON Object
        /// </example>
        [ResponseType(typeof(void))]
        [HttpPost]
        public IHttpActionResult UpdateProduct(int id, [FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.ProductID)
            {
                return BadRequest();
            }

            db.Entry(product).State = EntityState.Modified;
            // Picture update is handled by another method
            db.Entry(product).Property(p => p.ProductHasPic).IsModified = false;
            db.Entry(product).Property(p => p.PicExtension).IsModified = false;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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
        /// Receives Product picture data, uploads it to the webserver and updates the product's HasPic option
        /// </summary>
        /// <param name="id">the Product id</param>
        /// <returns>status code 200 if successful.</returns>
        /// <example>
        /// curl -F productpic=@file.jpg "https://localhost:xx/api/productdata/updateproductpic/2"
        /// POST: api/ProductData/UpdateProductPic/3
        /// HEADER: enctype=multipart/form-data
        /// FORM-DATA: image
        /// </example>
        /// https://stackoverflow.com/questions/28369529/how-to-set-up-a-web-api-controller-for-multipart-form-data

        [HttpPost]
        public IHttpActionResult UpdateProductPic(int id)
        {

            bool haspic = false;
            string picextension;
            if (Request.Content.IsMimeMultipartContent())
            {

                int numfiles = HttpContext.Current.Request.Files.Count;

                //Check if a file is posted
                if (numfiles == 1 && HttpContext.Current.Request.Files[0] != null)
                {
                    var ProductPic = HttpContext.Current.Request.Files[0];
                    //Check if the file is empty
                    if (ProductPic.ContentLength > 0)
                    {
                        var valtypes = new[] { "jpeg", "jpg", "png", "gif" };
                        var extension = Path.GetExtension(ProductPic.FileName).Substring(1);
                        //Check the extension of the file
                        if (valtypes.Contains(extension))
                        {
                            try
                            {
                                //file name is the id of the image
                                string fn = id + "." + extension;

                                //get a direct file path to ~/Content/Products/{id}.{extension}
                                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Products/"), fn);

                                //save the file
                                ProductPic.SaveAs(path);

                                //if these are all successful then we can set these fields
                                haspic = true;
                                picextension = extension;

                                //Update the product haspic and picextension fields in the database
                                Product SelectedProduct = db.Products.Find(id);
                                SelectedProduct.ProductHasPic = haspic;
                                SelectedProduct.PicExtension = extension;
                                db.Entry(SelectedProduct).State = EntityState.Modified;

                                db.SaveChanges();

                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Product Image was not saved successfully.");
                                Debug.WriteLine("Exception:" + ex);
                            }
                        }
                    }

                }
            }

            return Ok();
        }

        /// <summary>
        /// Adds a Product to the database.
        /// </summary>
        /// <param name="product">A Product object. Sent as POST form data.</param>
        /// <returns>status code 200 if successful. 400 if unsuccessful</returns>
        /// <example>
        /// POST: api/ProductData/AddProduct
        ///  FORM DATA: Product JSON Object
        /// </example>
        [ResponseType(typeof(Product))]
        [HttpPost]
        public IHttpActionResult AddProduct([FromBody] Product product)
        {
            //Will Validate according to data annotations specified on model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Products.Add(product);
            db.SaveChanges();

            return Ok(product.ProductID);
        }

        /// <summary>
        /// Deletes a product in the database
        /// </summary>
        /// <param name="id">The id of the product to delete.</param>
        /// <returns>200 if successful. 404 if not successful.</returns>
        /// <example>
        /// POST: api/ProductData/DeleteProduct/5
        /// </example>
        [HttpPost]
        public IHttpActionResult DeleteProduct(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            if (product.ProductHasPic && product.PicExtension != "")
            {
                //also delete image from path
                string path = HttpContext.Current.Server.MapPath("~/Content/Products/" + id + "." + product.PicExtension);
                if (System.IO.File.Exists(path))
                {
             
                    System.IO.File.Delete(path);
                }
            }

            db.Products.Remove(product);
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
        /// Finds a product in the system. Internal use only.
        /// </summary>
        /// <param name="id">The product id</param>
        /// <returns>TRUE if the product exists, false otherwise.</returns>
        private bool ProductExists(int id)
        {
            return db.Products.Count(e => e.ProductID == id) > 0;
        }
    }
}
