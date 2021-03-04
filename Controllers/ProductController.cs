using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Web.Script.Serialization;
using Order_Application.Models;
using Order_Application.Models.ViewModel;

namespace Order_Application.Controllers
{
    ///Reference for the project obtained from https://github.com/christinebittle/varsity_mvp.git/commit/8467279ed35c72dbac3d1cb86349301aa89d159a , Feb 2021 accessed. 
    public class ProductController : Controller
    {
        ///Http Client is the proper way to connect to a webapi
        //https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-5.0

        private JavaScriptSerializer jss = new JavaScriptSerializer();
        private static readonly HttpClient client;


        static ProductController()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            };
            client = new HttpClient(handler);
            //change this to match your own local port number
            client.BaseAddress = new Uri("https://localhost:44322/api/");
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));


            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ACCESS_TOKEN);

        }

        //Plain List of Products Action
        /*/ <summary>
        /// To retrieve data of all Product in the database in a list form
        /// </summary>
        /// <returns>Return a List of product with their information else return to Error view.</returns>
        // GET: Product/List
        public ActionResult List()
        {
            //Sending productlistResponse request to data controller, if request send succeed (status code 200), a list of product information will displayed.
            //If failed, direct to Error action (View)
            string url = "productdata/getproducts";
            HttpResponseMessage productlistResponse = client.GetAsync(url).Result;
            if (productlistResponse.IsSuccessStatusCode)
            {
                IEnumerable<ProductDto> SelectedProducts = productlistResponse.Content.ReadAsAsync<IEnumerable<ProductDto>>().Result;
                return View(SelectedProducts);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }*/

        // GET: Product/List?{PageNum}
        // If the page number is not included, set it to 0
        public ActionResult List(int PageNum = 0)
        {
            // Grab all products
            string url = "productdata/getproducts";
            // Send off an HTTP request
            // GET : /api/productdata/getproducts
            // Retrieve response
            HttpResponseMessage getproductResponse = client.GetAsync(url).Result;
            // If the response is a success, proceed
            if (getproductResponse.IsSuccessStatusCode)
            {
                // Fetch the response content into IEnumerable<ProductDto>
                IEnumerable<ProductDto> SelectedProducts = getproductResponse.Content.ReadAsAsync<IEnumerable<ProductDto>>().Result;

                // -- Start of Pagination Algorithm --

                // Find the total number of products
                int ProductCount = SelectedProducts.Count();
                // Number of products to display per page
                int PerPage = 6;
                // Determines the maximum number of pages (rounded up), assuming a page 0 start.
                int MaxPage = (int)Math.Ceiling((decimal)ProductCount / PerPage) - 1;

                // Lower boundary for Max Page
                if (MaxPage < 0) MaxPage = 0;
                // Lower boundary for Page Number
                if (PageNum < 0) PageNum = 0;
                // Upper Bound for Page Number
                if (PageNum > MaxPage) PageNum = MaxPage;

                // The Record Index of our Page Start
                int StartIndex = PerPage * PageNum;

                //Helps us generate the HTML which shows "Page 1 of ..." on the list view
                ViewData["PageNum"] = PageNum;
                ViewData["PageSummary"] = " " + (PageNum + 1) + " of " + (MaxPage + 1) + " ";

                // -- End of Pagination Algorithm --


                // Send back another request to get products, this time according to our paginated logic rules
                // GET api/productdata/getproductspage/{startindex}/{perpage}
                url = "productdata/getproductspage/" + StartIndex + "/" + PerPage;
                getproductResponse = client.GetAsync(url).Result;

                // Retrieve the response of the HTTP Request
                IEnumerable<ProductDto> SelectedProductsPage = getproductResponse.Content.ReadAsAsync<IEnumerable<ProductDto>>().Result;

                //Return the paginated of products instead of the entire list
                return View(SelectedProductsPage);

            }
            else
            {
                // If we reach here something went wrong with our list algorithm
                return RedirectToAction("Error");
            }
        }


        /// <summary>
        /// To gather information of particular product with selected product ID
        /// </summary>
        /// <param name="id">ProductID</param>
        /// <returns>Return information about slected ProductID. </returns>
        // GET: Product/Details/5
        public ActionResult Details(int id)
        {
            ShowProduct ViewModel = new ShowProduct();
            string findproducturl = "productdata/findproduct/" + id;
            HttpResponseMessage findproductResponse = client.GetAsync(findproducturl).Result;

            if (findproductResponse.IsSuccessStatusCode)
            {
                //Sending findproductResponse request to product data controller, 
                //if request send succeed (status code 200), get this product information (based on ProductDto.)
                //If failed, direct to Error action (View)

                ProductDto SelectedProduct = findproductResponse.Content.ReadAsAsync<ProductDto>().Result;
                ViewModel.product = SelectedProduct;

                return View(ViewModel);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }
           

        /// <summary>
        /// Create a New Product thru a form, includes Product Name, Product Description, Unit Price
        /// </summary>
        /// <returns>New customer request will be send and respond in POST Actions</returns>
        // GET: Product/Create 
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        /// <summary>
        /// Get the information on Create Product Form and update to the database
        /// </summary>
        /// <param name="ProductInfo"></param>
        /// <returns>If request succeed, New customer's information is created and added to the database.</returns>
        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Create(Product ProductInfo)
        {
            //Sending addproductresponse request to data controller (thru url string), 
            //If request send succeed (status code 200), add new product information.
            // & redirect to the  Details controller method, and add id to the url parameters
            //If failed, direct to Error action (View)

            string addproducturl = "ProductData/AddProduct";

            HttpContent content = new StringContent(jss.Serialize(ProductInfo));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage addproductResponse = client.PostAsync(addproducturl, content).Result;

            if (addproductResponse.IsSuccessStatusCode)
            {

                int Productid = addproductResponse.Content.ReadAsAsync<int>().Result;
                return RedirectToAction("Details", new { id = Productid });
            }
            else
            {
                return RedirectToAction("Error");
            }


        }
        /// <summary>
        /// Add new information of selected product thru the GET method;
        /// Required to retrieved latest data before updates are available.
        /// </summary>
        /// <param name="id">ProductID</param>
        /// <returns>Retreive the data of selected product and apply the changes and submit thru POST method.</returns>
        // GET: Product/Edit/5
        public ActionResult Edit(int id)
        {
            //Sending getupdateproductResponse request to data controller (thru url string), 
            //If request send succeed (status code 200), get the product information in edit view.
            //If failed, direct to Error action (View)
            UpdateProduct ViewModel = new UpdateProduct();

            string updateproducturl = "productdata/findproduct/" + id;
            HttpResponseMessage getupdateproductResponse = client.GetAsync(updateproducturl).Result;

            if (getupdateproductResponse.IsSuccessStatusCode)
            {
                //Put data into product data transfer object
                ProductDto SelectedProduct = getupdateproductResponse.Content.ReadAsAsync<ProductDto>().Result;
                ViewModel.product= SelectedProduct;

                return View(ViewModel);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // <summary>
        /// Received the changes and apply the update to the selected product
        /// </summary>
        /// <param name="id">ProductID</param>
        /// <param name="ProductInfo"></param>
        /// <returns>THe selected productinformation will be updated, include picture if have it</returns>
        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Edit(int id, Product ProductInfo, HttpPostedFileBase ProductPic)
        {

            string updateproducturl = "productdata/updateproduct/" + id;

            HttpContent content = new StringContent(jss.Serialize(ProductInfo));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage postupdateproductResponse = client.PostAsync(updateproducturl, content).Result;

            if (postupdateproductResponse.IsSuccessStatusCode)
            {
                //only attempt to send Product picture data if we have it
                if (ProductPic != null)
                {
                    //Send over image data for product
                    updateproducturl = "productdata/updateproductpic/" + id;

                    MultipartFormDataContent requestcontent = new MultipartFormDataContent();
                    HttpContent imagecontent = new StreamContent(ProductPic.InputStream);
                    requestcontent.Add(imagecontent, "ProductPic", ProductPic.FileName);
                    postupdateproductResponse = client.PostAsync(updateproducturl, requestcontent).Result;
                }

                return RedirectToAction("Details", new { id = id });
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        /// <summary>
        /// Delete selected peoduct with customerID
        /// </summary>
        /// <param name="id">ProductID</param>
        /// <returns>The product database will delete the product record</returns>
        // GET: Product/Delete/5
        [HttpGet]
        public ActionResult DeleteConfirm(int id)
        {
            //Sending response request to product data controller (thru url string), 
            //If request send succeed (status code 200), delete the product information.
            // & redirect to the slected view.
            //If failed, direct to Error action (View)

            string getdeleteproducturl = "productdata/findproduct/" + id;
            HttpResponseMessage getdeleteproductresponse = client.GetAsync(getdeleteproducturl).Result;

            if (getdeleteproductresponse.IsSuccessStatusCode)
            {
                //Put data into product data transfer object
                ProductDto SelectedProduct = getdeleteproductresponse.Content.ReadAsAsync<ProductDto>().Result;
                return View(SelectedProduct);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        /// <summary>
        /// Delete selected product thru POST method
        /// </summary>
        /// <param name="id">ProductID</param>
        /// <returns>Selected product record will be deleted and return to "List" Action to view the updated database</returns>
        // POST: Product/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Delete(int id)
        {
            string url = "productdata/deleteproduct/" + id;
            //post body is empty
            HttpContent content = new StringContent("");
            HttpResponseMessage response = client.PostAsync(url, content).Result;
         
            if (response.IsSuccessStatusCode)
            {

                return RedirectToAction("List");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }
        /// <summary>
        /// Error Action
        /// </summary>
        /// <returns>It will display error view</returns>
        public ActionResult Error()
        {
            return View();
        }
    }
}

