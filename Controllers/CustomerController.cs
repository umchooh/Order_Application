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
    public class CustomerController : Controller
    {
        private JavaScriptSerializer jss = new JavaScriptSerializer();
        private static readonly HttpClient client;

        static CustomerController()
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
        /// To retrieve data of all customers in the database in a list form
        /// </summary>
        /// <returns>Return a List of customer with their information else return to Error view.</returns>
        // GET: Customer/List  
        public ActionResult List()
        {
            //Sending customerlistresponse request to data controller, if request send succeed (status code 200), a list of customer information will displayed.
            //If failed, direct to Error action (View)
            string listcustomerurl = "customerdata/getcustomers";
            HttpResponseMessage customerlistresponse = client.GetAsync(listcustomerurl).Result;
            if (customerlistresponse.IsSuccessStatusCode)
            {
                IEnumerable<CustomerDto> SelectedCustomers = customerlistresponse.Content.ReadAsAsync<IEnumerable<CustomerDto>>().Result;
                return View(SelectedCustomers);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        /// <summary>
        /// To gather information of particular customer with selected customerID
        /// </summary>
        /// <param name="id">CustomerID</param>
        /// <returns>Return particular customer information. Using View Modal, set a limit on what can be view by the client side and also able to set [DisplayName] to change the column name that will appaered in the View.  </returns>
        // GET: Customer/Details/5
        public ActionResult Details(int id)
        {
            ShowCustomer ViewModel = new ShowCustomer();
            string findcustomerurl = "customerdata/findcustomer/" + id;
            HttpResponseMessage findcustomerresponse = client.GetAsync(findcustomerurl).Result;
           
            if (findcustomerresponse.IsSuccessStatusCode)
            {
                //Sending findcustomerresponse request to data controller, 
                //if request send succeed (status code 200), Please retienve me this customer information (based on CustomerDto.)
                //If failed, direct to Error action (View)

                CustomerDto SelectedCustomer = findcustomerresponse.Content.ReadAsAsync<CustomerDto>().Result;
                ViewModel.customer = SelectedCustomer;

                return View(ViewModel);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }
        /// <summary>
        /// Create a New Customer thru a form
        /// </summary>
        /// <returns>New customer request will be send and respond in POST Actions</returns>
        // GET: Customer/Create 
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        /// <summary>
        /// Get the information on Create Customer Form and update to the database
        /// </summary>
        /// <param name="CustomerInfo"></param>
        /// <returns>If request succeed, New customer's information is created and added to the database.</returns>
        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Create(Customer CustomerInfo)
        {
            //Sending addcustomerresponse request to data controller (thru url string), 
            //If request send succeed (status code 200), Please add new customer information.
            // & redirect to the  Details controller method, and add id to the url parameters
            //If failed, direct to Error action (View)

            string addcustomerurl = "CustomerData/AddCustomer";
            
            HttpContent content = new StringContent(jss.Serialize(CustomerInfo));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage addcustomerresponse = client.PostAsync(addcustomerurl, content).Result;

            if (addcustomerresponse.IsSuccessStatusCode)
            {

                int Customerid = addcustomerresponse.Content.ReadAsAsync<int>().Result;
                return RedirectToAction("Details", new { id = Customerid });
            }
            else
            {
                return RedirectToAction("Error");
            }


        }
        /// <summary>
        /// Add new information of selected customer thru the GET method;
        /// Required to retrieved latest data before updates are available.
        /// </summary>
        /// <param name="id">CustomerID</param>
        /// <returns>Retreive the data of selected customer and apply the changes and submit thru POST method.</returns>
        // GET: Customer/Edit/5
        public ActionResult Edit(int id)
        {
            //Sending getupdatecustomerresponse request to data controller (thru url string), 
            //If request send succeed (status code 200), Please retrieve the customer information in edit view.
            //If failed, direct to Error action (View)
            UpdateCustomer ViewModel = new UpdateCustomer();

            string getupdatecustomerurl = "customerdata/findcustomer/" + id;
            HttpResponseMessage findcustomerresponse = client.GetAsync(getupdatecustomerurl).Result;
         
            if (findcustomerresponse.IsSuccessStatusCode)
            {
                //Put data into CustomerDto
                CustomerDto SelectedCustomer = findcustomerresponse.Content.ReadAsAsync<CustomerDto>().Result;
                ViewModel.customer = SelectedCustomer;

                return View(ViewModel);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }
        /// <summary>
        /// Received the changes and apply the update to the selected customer
        /// </summary>
        /// <param name="id">CustomerID</param>
        /// <param name="CustomerInfo"></param>
        /// <returns>THe selected customer information will be updated</returns>
        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Edit(int id, Customer CustomerInfo)
        {
            //Sending postupdatecustomerresponse request to data controller (thru url string), 
            //If request send succeed (status code 200), Please save the changes (update) the customer information.
            // & redirect to the  Details controller method, and add id to the url parameters
            //If failed, direct to Error action (View)
            string postupdatecustomerurl = "customerdata/updatecustomer/" + id;

            HttpContent content = new StringContent(jss.Serialize(CustomerInfo));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage updatecustomerresponse = client.PostAsync(postupdatecustomerurl, content).Result;

            if (updatecustomerresponse.IsSuccessStatusCode)
            {
                return RedirectToAction("Details", new { id = id });
            }
            else
            {
                return RedirectToAction("Error");
            }
        }
        /// <summary>
        /// Delete selected Customer with customerID
        /// </summary>
        /// <param name="id">CustomerID</param>
        /// <returns>The customer database will delete the customer record</returns>
        // GET: customer/Delete/5
        [HttpGet]
        public ActionResult DeleteConfirm(int id)
        {
            //Sending getudeletecustomerresponse request to data controller (thru url string), 
            //If request send succeed (status code 200), delete the customer information.
            // & redirect to the slected customer view.
            //If failed, direct to Error action (View)

            string getdeletecustomerurl = "customerdata/findcustomer/" + id;
            HttpResponseMessage getdeletecustomerresponse = client.GetAsync(getdeletecustomerurl).Result;
            
            if (getdeletecustomerresponse.IsSuccessStatusCode)
            {
                //Put data into player data transfer object
                CustomerDto SelectedCustomer = getdeletecustomerresponse.Content.ReadAsAsync<CustomerDto>().Result;
                return View(SelectedCustomer);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }
        /// <summary>
        /// Delete selected customer thru POST method
        /// </summary>
        /// <param name="id">CustomerID</param>
        /// <returns>Selected customer record will be deleted and return to "List" Action to view the updated database</returns>
        // POST: Customer/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Delete(int id)
        {
            string postdeletecustomerurl = "customerdata/deletecustomer/" + id;
            
            HttpContent content = new StringContent("");
            HttpResponseMessage response = client.PostAsync(postdeletecustomerurl, content).Result;
            
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
