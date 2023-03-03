using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebAppsNoAuth.Data;
using WebAppsNoAuth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using MailKit.Security;
using MimeKit;
using Org.BouncyCastle.Crypto.Macs;
using System.Net.Mail;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAppsNoAuth.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly WebAppsNoAuthDbContext _webApps;
        private readonly IConfiguration _configuration;
        // private readonly SqlConnection _connection;

        private readonly ProviderController _providers;

        public HomeController(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
        {
            _webApps = webApps;
            _configuration = configuration;
            _providers = new ProviderController(webApps, configuration);
           // _connection = connection;

        }

        //THESE METHODS NEED TO BE MOVED
        public bool SendNewRequestEmail(int userId, int requestTypeId, DateTime startDate, DateTime endDate)
        {
            User currentUser = _providers.User.GetUserById(userId);
            var userName = currentUser.Name;
            var userEmail = currentUser.Email;

            return _providers.Email.SendNewRequestEmail(userId, requestTypeId, startDate, endDate, userName, userEmail);
        }

        public bool SendNewRequestEmailToManagers(int userId, int requestTypeId, DateTime startDate, DateTime endDate)
        {
            Debug.WriteLine("sendnewreqemailtomanagers"); ;
            EmailTemplate emailTemplate = new EmailTemplate();
            emailTemplate.Subject = "New Request";

            User currentUser = _providers.User.GetUserById(userId);
            var userName = currentUser.Name;
            var userEmail = currentUser.Email;
            var managerId = currentUser.ManagerUserId;
            if (managerId == -1)
            {
                return true;
            }

            User currentUserManager = _providers.User.GetUserById(managerId);
            var managerName = currentUserManager.Name;
            var managerEmail = currentUserManager.Email;

            return _providers.Email.SendNewRequestEmailToManagers(userId, requestTypeId, startDate, endDate, userName, managerId, managerName, managerEmail);
        }

        public List<User> GetAllUsersAsObject()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            //var institutionId = _providers.User.GetInstitutionId(userId);
            List<User> allUsers = _providers.User.GetAllUsersAsObject(userId);

            return allUsers;
        }

        public List<Location> GetAllUserLocations()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            List<Location> allUsers = _providers.User.GetAllUserLocations(userId);
            return allUsers;
        }
        //THE ABOVE METHODS NEED TO BE MOVED TO THEIR OWN SEPARATE CLASS 
        // GET: /<controller>/
        public IActionResult Index()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            User currentUser = _providers.User.GetUserById(userId);
            if (currentUser.Password == "password")
            {
                ViewData["resetPassword"] = true;
            }
            else
            {
                ViewData["resetPassword"] = false;

            }
            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;
            var statusTypes = GetAllStatusTypes();
            ViewData["StatusList"] = new SelectList(statusTypes, "StatusTypeId", "StatusTypeName");
            var userLocationsList = GetAllUserLocations();
            if (userLocationsList.Count != 0)
            {
                ViewData["UserLocationsList"] = new SelectList(userLocationsList, "LocationId", "LocationValue");
            }
            else
            {
                Location fake = new Location { LocationId = -1, LocationValue = "", LocationTitle = "" };
                List<Location> fakeLocationsList = new List<Location>();
                fakeLocationsList.Add(fake);
                ViewData["UserLocationsList"] = new SelectList(fakeLocationsList, "LocationId", "LocationValue");

            }
            return View();
        }

        public IActionResult Holidays()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            User currentUser = _providers.User.GetUserById(userId);

            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;
            var managerUsers = GetAllManagerUsers(userId);
            ViewData["ManagerUsersList"] = new SelectList(managerUsers, "Id", "Name");
            var requestTypes = GetAllRequestTypes();
            ViewData["RequestTypeList"] = new SelectList(requestTypes, "RequestTypeId", "RequestTypeName");

            var statusTypes = GetAllStatusTypes();
            ViewData["StatusList"] = new SelectList(statusTypes, "StatusTypeId", "StatusTypeName");
            var userLocationsList = GetAllUserLocations();
            if (userLocationsList.Count != 0)
            {
                ViewData["UserLocationsList"] = new SelectList(userLocationsList, "LocationId", "LocationValue");
            }
            else
            {
                Location fake = new Location { LocationId = -1, LocationValue = "", LocationTitle = "" };
                List<Location> fakeLocationsList = new List<Location>();
                fakeLocationsList.Add(fake);
                ViewData["UserLocationsList"] = new SelectList(fakeLocationsList, "LocationId", "LocationValue");
            }

            return View();
        }

        public IActionResult Calendar()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);

            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];

            var allUsers = GetAllUsersAsObject();
            var currentUser = _providers.User.GetUserById(userId);
            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;
            ViewData["UsersList"] = new SelectList(allUsers, "Id", "Name");
            var requestTypes = _providers.Request.GetAllRequestTypes();
            ViewData["RequestTypeList"] = new SelectList(requestTypes, "RequestTypeId", "RequestTypeName");

            var statusTypes = GetAllStatusTypes();
            ViewData["StatusList"] = new SelectList(statusTypes, "StatusTypeId", "StatusTypeName");
            var userLocationsList = GetAllUserLocations();
            if (userLocationsList.Count != 0)
            {
                ViewData["UserLocationsList"] = new SelectList(userLocationsList, "LocationId", "LocationValue");
            }
            else
            {
                Location fake = new Location { LocationId = -1, LocationValue = "", LocationTitle = "" };
                List<Location> fakeLocationsList = new List<Location>();
                fakeLocationsList.Add(fake);
                ViewData["UserLocationsList"] = new SelectList(fakeLocationsList, "LocationId", "LocationValue");
            }

            return View();
        }
        public IActionResult Privacy()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            ViewData["Admin"] = _providers.User.IsUserAdmin(userId);
            ViewData["Manager"] = _providers.User.IsUserManager(userId);

            var statusTypes = GetAllStatusTypes();
            ViewData["StatusList"] = new SelectList(statusTypes, "StatusTypeId", "StatusTypeName");
            var userLocationsList = GetAllUserLocations();
            ViewData["UserLocationsList"] = new SelectList(userLocationsList, "LocationId", "LocationName");
            return View();
        }

        public async Task<IActionResult> LogOut()
        {
            //Add additional logic here to confirm if user wants to log out
            ViewData["Authenticated"] = "true"; //?
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            ViewData["Admin"] = _providers.User.IsUserAdmin(userId);
            ViewData["Manager"] = _providers.User.IsUserManager(userId);

            var statusTypes = GetAllStatusTypes();
            ViewData["StatusList"] = new SelectList(statusTypes, "StatusTypeId", "StatusTypeName");
            var userLocationsList = GetAllUserLocations();
            if (userLocationsList.Count != 0)
            {
                ViewData["UserLocationsList"] = new SelectList(userLocationsList, "LocationId", "LocationValue");
            }
            else
            {
                Location fake = new Location { LocationId = -1, LocationValue = "", LocationTitle = "" };
                List<Location> fakeLocationsList = new List<Location>();
                fakeLocationsList.Add(fake);
                ViewData["UserLocationsList"] = new SelectList(fakeLocationsList, "LocationId", "LocationValue");

            }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Access");
        }

        public IActionResult ResetPassword()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            User currentUser = _providers.User.GetUserById(userId);
            if (currentUser.Password == "password")
            {
                ViewData["resetPassword"] = true;
            }
            else
            {
                ViewData["resetPassword"] = false;

            }
            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;
            var statusTypes = GetAllStatusTypes();
            ViewData["StatusList"] = new SelectList(statusTypes, "StatusTypeId", "StatusTypeName");
            var userLocationsList = GetAllUserLocations();
            if (userLocationsList.Count != 0)
            {
                ViewData["UserLocationsList"] = new SelectList(userLocationsList, "LocationId", "LocationValue");
            }
            else
            {
                Location fake = new Location { LocationId = -1, LocationValue = "", LocationTitle = "" };
                List<Location> fakeLocationsList = new List<Location>();
                fakeLocationsList.Add(fake);
                ViewData["UserLocationsList"] = new SelectList(fakeLocationsList, "LocationId", "LocationValue");

            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public List<User> GetAllManagerUsers(int managerUserId) //get all the users who the manager manages 
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            List<User> allUsers = new List<User>();
            allUsers = _providers.Manager.GetAllManagerUsers(managerUserId);

            return allUsers;
        }

        public List<RequestType> GetAllRequestTypes()
        {
            return _providers.Request.GetAllRequestTypes();
        }
        public List<StatusType> GetAllStatusTypes()
        {
            return _providers.User.GetAllStatusTypes();
        }
        public ActionResult GetAllRequests()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            List<Request> allRequests = _providers.Request.GetAllRequests(userId);

            return Json(new { data = allRequests });
        }
        public ActionResult GetAllApprovedRequests(int userId)
        {
            List<Request> allRequests = _providers.Request.GetAllApprovedRequests(userId);
            return Json(new { data = allRequests });
        }
        //Manager getting another users requests
        public ActionResult GetUserRequests(int userId)
        {
            List<Request> allRequests = _providers.Request.GetUserRequests(userId);
            return Json(new { data = allRequests });
        }

        public List<Request> GetUserRequestsAsObject(int userId)
        {
            List<Request> allRequests = _providers.Request.GetUserRequests(userId);
            return allRequests;
        }
        public int GetUserEntitlements(int userId)
        {
            Debug.WriteLine("getuserentitlements");
            return _providers.User.GetUserEntitlements(userId);
        }
        public bool AddNewRequest(int userId, int requestTypeId, DateTime startDate, DateTime endDate)
        {
            var success = _providers.Request.AddNewRequest(userId, requestTypeId, startDate, endDate);
            if (success)
            {
                var success2 = SendNewRequestEmail(userId, requestTypeId, startDate, endDate);
                if (success2)
                {
                    var success3 = SendNewRequestEmailToManagers(userId, requestTypeId, startDate, endDate);
                    if (success3) //do we need this nesting or is one success enough?
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        public bool DeleteRequest(int requestId)
        {
            return _providers.Request.DeleteRequest(requestId);
        }

        public ActionResult ValidateRequest(int userId, int requestTypeId, DateTime startDate, DateTime endDate)
        {
            var currentUserId = -1;
            if (userId == -1)
            {
                currentUserId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            }
            else
            {
                currentUserId = userId;
            }
            // toReturn.Id = 1 --> valid request
            // toReturn.Id = 3 --> invalid request
            Message toReturn = new Message();
            var totalAmount = GetUserEntitlements(currentUserId);
            var requestsList = GetUserRequestsAsObject(currentUserId);
            var usedAmount = 0;

            var requestAmount = 0;

            for (var i=startDate; i<=endDate; i = i.AddDays(1))
            {
                var dayOfWeek = i.ToString("dddd");
                if (dayOfWeek.Equals("Saturday") || dayOfWeek.Equals("Sunday")) //Don't count Weekend 
                {
                    continue;
                }
                else
                {
                    requestAmount += 1;
                }
            }
            
            Debug.WriteLine("NUMBER OF DAYS IN REQ: " + requestAmount);

            //Check if enough entitlements for the year, if the requestType is 'Annual'
            if (requestTypeId == 1)
            {
                Debug.WriteLine(requestsList);
                for (var i = 0; i < requestsList.Count(); i++)
                {
                    if (requestsList[i].RequestTypeId == 1)
                    {
                        if (requestsList[i].ApprovedMessage == "Rejected")
                        {
                            continue; //Don't count rejected requests
                        }
                       if ((requestsList[i].EndDate - requestsList[i].StartDate).Days == 0) //one day's req 
                       {
                            usedAmount += 1;
                            Debug.WriteLine(usedAmount);
                            continue;
                       }
                       // usedAmount += days;
                       for (var j = requestsList[i].StartDate; j <= requestsList[i].EndDate; j = j.AddDays(1))
                       {
                            var dayOfWeek = j.ToString("dddd");
                            Debug.WriteLine(dayOfWeek);
                            if (dayOfWeek.Equals("Saturday") || dayOfWeek.Equals("Sunday")) //Don't count Weekend 
                            {
                                continue;
                            }
                            else
                            {
                                usedAmount += 1;
                            }
                        }
                    }
                }
                Debug.WriteLine(usedAmount + " is used amount");
                if (usedAmount + requestAmount > totalAmount)
                {
                    toReturn.Id = 3;
                    toReturn.MessageStr = "Not enough entitlements.";
                    return Json(new { data = toReturn });
                }
            }

            //Check if any overlaps with previous requests
            var overlappingRequests = requestsList.Where(r => r.StartDate < endDate && startDate < r.EndDate);
            if (overlappingRequests.Count() > 0)
            {
                toReturn.Id = 3;
                toReturn.MessageStr = "Overlaps with previous requests.";
                return Json(new { data = toReturn });
            }
            var success = AddNewRequest(currentUserId, requestTypeId, startDate, endDate);
            Debug.WriteLine("validatereq result of addnewreq: " + success);
            if (success)
            {
                toReturn.Id = 1;
                toReturn.MessageStr = "";
                return Json(new { data = toReturn });
            }
            Message errorMessage = new Message
            {
                Id = -1,
                MessageStr = "There was an error with processing your request."
            };
            return Json(new { data = errorMessage });

        }

        public bool SetUserStatus(int statusTypeId, int locationId, bool isWFH, string wfhContact)
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            return _providers.User.SetUserStatus(userId, statusTypeId, locationId, isWFH, wfhContact);
        }

        public ActionResult GetUserStatus(int userId) //problem here 
        {
            var currentUserId = -1;
            if (userId == -1)
            {
                currentUserId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            }
            else
            {
                currentUserId = userId;
            }
            Status userStatus = _providers.User.GetUserStatus(currentUserId);
            return Json(new { data = userStatus });
        }

        public ActionResult GetAllUsersStatus() 
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);

            List<Status> allUsersStatus = _providers.User.GetAllUsersStatus(userId);
            return Json(new { data = allUsersStatus });
        }

        public bool SetUserPassword(string password)
        {
            int currentUserId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            return _providers.User.SetUserPassword(currentUserId, password);
        }
    }
}

