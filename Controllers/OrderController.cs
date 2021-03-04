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
    public class OrderController : Controller
    {
        //Http Client is the proper way to connect to a webapi
        //https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-5.0

        private JavaScriptSerializer jss = new JavaScriptSerializer();
        private static readonly HttpClient client;


        static OrderController()
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
        // <summary>
        /// To retrieve data of all orders in the database in a list form
        /// </summary>
        /// <returns>Return a List of orders with their information include Customer First Name and Last Name and when the order placed.</returns>
        // GET: Order/List
        public ActionResult List()
        {
            //Send listorderResponse to data controller, if request send succeed (status code 200), A list of Orders Informotation will displayed.
            //If failed, direct to Error action (View)
            string getordersUrl = "Orderdata/getorders";
            string findcustomerUrl = "CustomerData/FindCustomer/";
            HttpResponseMessage listorderResponse = client.GetAsync(getordersUrl).Result;
            if (listorderResponse.IsSuccessStatusCode)//if request success
            {
                IEnumerable<OrderDto> SelectedOrders = listorderResponse.Content.ReadAsAsync<IEnumerable<OrderDto>>().Result;// show me a list of orders
                foreach(OrderDto orderDto in SelectedOrders)
                {
                    HttpResponseMessage customerResponse = client.GetAsync(findcustomerUrl + orderDto.CustomerID).Result;//send a request to find the customer information on each order that need to displayed
                    if (customerResponse.IsSuccessStatusCode)
                    {
                        CustomerDto customer = customerResponse.Content.ReadAsAsync<CustomerDto>().Result;
                        orderDto.Customer = customer;
                    }
                    
                }
                return View(SelectedOrders);//return the orders
            }
            else
            {
                return RedirectToAction("Error");//return to Error action if request is fail.
            }
        }

        /// <summary>
        /// Show particular order's details : Customer Name (first name and Last name), Ordered Date and a table to show the items ordered with its price.
        /// In the table , it include list of product(item) name, unit price, quantity and total price of per item. At the end of the table include a Total of Subtotal.
        /// </summary>
        /// <param name="Orderid">OrderID</param>
        /// <returns>A simiilar to invoice form, customer information and table showed items in the order.</returns>
        //GET: Order/Details/5
        public ActionResult Details(int id)
        {
            ///Send orderResponse to find that order data; 
            ShowOrder ViewModel = new ShowOrder();
            string orderUrl = "orderdata/findorder/" + id;
            HttpResponseMessage orderResponse = client.GetAsync(orderUrl).Result;
            if (orderResponse.IsSuccessStatusCode)
            {
                //Step 1: find Order information
                //if requested is OK, place the selected orderid's data into OrderDto
                OrderDto SelectedOrder = orderResponse.Content.ReadAsAsync<OrderDto>().Result;

                //Step 2: find the customer information for that makes the order
                ////if requested is OK, place the selected matched customerid's data into CustomerDto
                string customerUrl = "CustomerData/FindCustomer/" + SelectedOrder.CustomerID;
                HttpResponseMessage customerResponse = client.GetAsync(customerUrl).Result;

                if (customerResponse.IsSuccessStatusCode)
                {
                    CustomerDto customer = customerResponse.Content.ReadAsAsync<CustomerDto>().Result;
                    SelectedOrder.Customer = customer;
                }

                ViewModel.Order = SelectedOrder;

                //Step 3: find the product information that is listed in this order thru the briding table "OrderItems"
                ////if requested is OK, add the list of items according to the order, contains details cusch as product name, amount, and price into OrderItemsDto
                string orderItemsUrl = "OrderItemsData/FindItemsOfOrder/" + id;
                HttpResponseMessage orderItemsResponse = client.GetAsync(orderItemsUrl).Result;

                if (orderItemsResponse.IsSuccessStatusCode)
                {
                    List<OrderItemDto> orderItemDtos = new List<OrderItemDto>();
                    IEnumerable<OrderItems> orderItems = orderItemsResponse.Content.ReadAsAsync<IEnumerable<OrderItems>>().Result;
                    foreach (OrderItems item in orderItems)
                    {
                        string productUrl = "ProductData/FindProduct/" + item.ProductID;
                        HttpResponseMessage productResponse = client.GetAsync(productUrl).Result;
                        if (productResponse.IsSuccessStatusCode)
                        {
                            ProductDto product = productResponse.Content.ReadAsAsync<ProductDto>().Result;
                            OrderItemDto orderItemDto = new OrderItemDto
                            {
                                Product = product,
                                ProductPrice = (double)item.ProductPrice,
                                Amount = (decimal)item.Amount
                            };
                            orderItemDtos.Add(orderItemDto);
                        }
                    }

                    ViewModel.OrderItems = orderItemDtos;
                }
                //In the ViewModel, it should now contains all three entity information and ready to be call out to display to customer.
                return View(ViewModel);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }
        /// This method require more steps to gether information from the OrderItem Bridging table to list the ordered items and apply changes to the order.
        /// Currently, just listed the orderdate and customer ID
        /// <summary>
        /// Create a New order thru a form
        /// </summary>
        /// <returns>New order request will be send and respond in POST Actions</returns>

        // GET: Order/Create 
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        /// This method require more steps to gether information from the OrderItem Bridging table to list the ordered items and apply changes to the order.
        /// Currently, just listed the orderdate and customer ID
        // <summary>
        /// Get the information on Create Order Form and update to the database
        /// </summary>
        /// <param name="OrderInfo"></param>
        /// <returns>If request succeed, New order's information is created and added to the database.</returns>

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Create(Order OrderInfo)
        {
            string url = "OrderData/AddOrder";
            HttpContent content = new StringContent(jss.Serialize(OrderInfo));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;

            if (response.IsSuccessStatusCode)
            {

                int Orderid = response.Content.ReadAsAsync<int>().Result;
                return RedirectToAction("Details", new { id = Orderid });
            }
            else
            {
                return RedirectToAction("Error");
            }


        }
        /// This method require more steps to gether information from the OrderItem Bridging table to list the ordered items and apply changes to the order.
        /// Currently, just listed the orderdate and customer ID
        /// <summary> 
        /// Add new information of selected order thru the GET method;
        /// Required to retrieved latest data before updates are available.
        /// </summary>
        /// <param name="id">OrderID</param>
        /// <returns>Retreive the data of selected order and apply the changes and submit thru POST method.</returns>
        // GET: Order/Edit/5
        public ActionResult Edit(int id)
        {
            //Sending getupdateorderResponse request to data controller (thru url string), 
            //If request send succeed (status code 200), Please retrieve the customer information in edit view.
            //If failed, direct to Error action (View)

            UpdateOrder ViewModel = new UpdateOrder();

            string url = "orderdata/findorder/" + id;
            HttpResponseMessage getupdateorderResponse = client.GetAsync(url).Result;
            if (getupdateorderResponse.IsSuccessStatusCode)
            {
                //If request is OK, place new update data in OrderDto
                OrderDto SelectedOrder = getupdateorderResponse.Content.ReadAsAsync<OrderDto>().Result;
                ViewModel.order = SelectedOrder;


                return View(ViewModel);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        /// This method require more steps to gether information from the OrderItem Bridging table to list the ordered items and apply changes to the order.
        /// Currently, just listed the orderdate and customer ID
        /// <summary>
        /// Received the changes and apply the update to the selected order 
        /// </summary>
        /// <param name="id">OrderID</param>
        /// <param name="OrderInfo"></param>
        /// <returns>THe selected order information will be updated</returns>
        // POST: Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Edit(int id, Order OrderInfo)
        {
            //Sending postupdateorderResponse request to data controller (thru url string), 
            //If request send succeed (status code 200), Please save the changes (update) the order information (further include OrderItemsDto Data).
            // & redirect to the  Details controller method, and add id to the url parameters
            //If failed, direct to Error action (View)
            string url = "orderdata/updateorder/" + id;
            
            HttpContent content = new StringContent(jss.Serialize(OrderInfo));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage postupdateorderResponse = client.PostAsync(url, content).Result;
           
            if (postupdateorderResponse.IsSuccessStatusCode)
            {
                return RedirectToAction("Details", new { id = id });
            }
            else
            {
                return RedirectToAction("Error");
            }
        }
        /// <summary>
        /// Delete selected Order with orderID
        /// </summary>
        /// <param name="id">OrderID</param>
        /// <returns>The order database will delete the order record</returns>
        // GET: Order/Delete/5
        [HttpGet]
        public ActionResult DeleteConfirm(int id)
        {
            //Step 1. Send request to find the selecter order that need to be delete
            //If request is success,
            string url = "orderdata/findorder/" + id;
            HttpResponseMessage getdeleteorderResponse = client.GetAsync(url).Result;
           
            if (getdeleteorderResponse.IsSuccessStatusCode)
            {
                //Place that order data into the OrderDto
                OrderDto SelectedOrder = getdeleteorderResponse.Content.ReadAsAsync<OrderDto>().Result;
                //Step2. To allocated the customer with that order 
                string findcustomerUrl = "CustomerData/FindCustomer/" + SelectedOrder.CustomerID;
                HttpResponseMessage findcustomerResponse = client.GetAsync(findcustomerUrl).Result;

                if (findcustomerResponse.IsSuccessStatusCode)
                {
                    //if request is success, then add that customer data into CustomerDto
                    CustomerDto customer = findcustomerResponse.Content.ReadAsAsync<CustomerDto>().Result;
                    SelectedOrder.Customer = customer;
                }


                return View(SelectedOrder);

            }
            else
            {
                return RedirectToAction("Error");
            }
        }
        /// <summary>
        /// Delete selected order thru POST method
        /// </summary>
        /// <param name="id">OrderID</param>
        /// <returns>Selected order record will be deleted and return to "List" Action to view the updated database</returns>
        // POST: Order/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Delete(int id)
        {
            ///Once received the request from GET method, it will connect to the database and it will return empty content;
            ///then direct client back to the updated list.
            string url = "orderdata/deleteorder/" + id;
            //post body is empty
            HttpContent content = new StringContent("");
            HttpResponseMessage deleteorderResponse = client.PostAsync(url, content).Result;
            if (deleteorderResponse.IsSuccessStatusCode)
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
