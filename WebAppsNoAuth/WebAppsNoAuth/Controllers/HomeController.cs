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
using Microsoft.Build.ObjectModelRemoting;
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
        public bool SendNewRequestEmail(int userId, int requestTypeId, DateTime startDate, DateTime endDate, string requestDescription)
        {
            User currentUser = _providers.User.GetUserById(userId);
            var userName = currentUser.Name;
            var userEmail = currentUser.Email;

            return _providers.Email.SendNewRequestEmail(userId, requestTypeId, startDate, endDate, userName, userEmail, requestDescription);
        }

        public bool SendNewRequestEmailToManagers(int userId, int requestTypeId, DateTime startDate, DateTime endDate, string requestDescription)
        {
            User currentUser = _providers.User.GetUserById(userId);
            var userName = currentUser.Name;
            //var userEmail = currentUser.Email;
            var managerId = currentUser.ManagerUserId;
            if (managerId == -1)
            {
                return true;
            }

            User currentUserManager = _providers.User.GetUserById(managerId);
            var managerName = currentUserManager.Name;
            var managerEmail = currentUserManager.Email;

            return _providers.Email.SendNewRequestEmailToManagers(userId, requestTypeId, startDate, endDate, userName, managerId, managerName, managerEmail, requestDescription);
        }

        public void SendMeetingInviteEmail(int userId, int hostUserId, string title, string description, string priority, DateTime meetingDate, string startTime, string endTime)
        {
            User currentUser = _providers.User.GetUserById(userId);
            var userName = currentUser.Name;
            var userEmail = currentUser.Email;
            User hostUser = _providers.User.GetUserById(hostUserId);
            var hostName = hostUser.Name;

            _providers.Email.SendMeetingInviteEmail(userId, userName, userEmail, hostName, title, description, priority, meetingDate, startTime, endTime);
        }

        public void SendAcceptMeetingEmailToHost(int userId, int meetingId)
        {
            Meeting meeting = _providers.User.GetMeetingById(meetingId);
            var meetingTitle = meeting.Title;

            var hostUserId = meeting.HostUserId;
            var hostUser = _providers.User.GetUserById(hostUserId);
            var hostName = hostUser.Name;
            var hostEmail = hostUser.Email;

            var attendee = _providers.User.GetUserById(userId);
            var attendeeName = attendee.Name;

            _providers.Email.SendAcceptMeetingEmailToHost(meetingTitle, hostName, hostEmail, attendeeName);
        }

        public void SendRejectMeetingEmailToHost(int userId, int meetingId)
        {
            Meeting meeting = _providers.User.GetMeetingById(meetingId);
            var meetingTitle = meeting.Title;

            var hostUserId = meeting.HostUserId;
            var hostUser = _providers.User.GetUserById(hostUserId);
            var hostName = hostUser.Name;
            var hostEmail = hostUser.Email;

            var attendee = _providers.User.GetUserById(userId);
            var attendeeName = attendee.Name;

            _providers.Email.SendRejectMeetingEmailToHost(meetingTitle, hostName, hostEmail, attendeeName);
        }

        public void SendCancelMeetingEmail(int userId, int meetingId)
        {
            Meeting meeting = _providers.User.GetMeetingById(meetingId);
            var meetingTitle = meeting.Title;

            var hostUserId = meeting.HostUserId;
            var hostUser = _providers.User.GetUserById(hostUserId);
            var hostName = hostUser.Name;

            var attendee = _providers.User.GetUserById(userId);
            var attendeeName = attendee.Name;
            var attendeeEmail = attendee.Email;

            _providers.Email.SendCancelMeetingEmail(meetingTitle, attendeeName, attendeeEmail, hostName);
        }

        public IEnumerable<User> GetAllUsersAsObject()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            //var institutionId = _providers.User.GetInstitutionId(userId);
            IEnumerable<User> allUsers = _providers.User.GetAllUsersAsObject(userId);

            return allUsers;
        }
        public ActionResult GetAllUsersAsJson()
        {
            IEnumerable<User> allUsers = GetAllUsersAsObject();
            return Json(new { data = allUsers });
        }
        public ActionResult GetAllUsersAsJsonMeeting()
        {
            IEnumerable<User> allUsers = GetAllUsersAsObject();
            var allUsersExHost = new List<User>();
            for (var i = 1; i < allUsers.Count(); i++)
            {
                allUsersExHost.Add(allUsers.ElementAt(i));
            }
            return Json(new { data = allUsersExHost });
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

        public IActionResult Meetings()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);

            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];

            var currentUser = _providers.User.GetUserById(userId);
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

        public IActionResult MyProjects()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);

            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];

            var allUsers = GetAllUsersAsObject();
            var allUsersExHost = new List<User>();
            for (var i = 1; i < allUsers.Count(); i++)
            {
                allUsersExHost.Add(allUsers.ElementAt(i));
            }
            var currentUser = _providers.User.GetUserById(userId);
            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;
            ViewData["UsersList"] = new SelectList(allUsersExHost, "Id", "Name");

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
            ViewData["NumberOfProjects"] = GetAllProjects().Count;
            return View();
        }

        public IActionResult PMTasks()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);

            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];

            var allUsers = GetAllUsersAsObject();
            var currentUser = _providers.User.GetUserById(userId);
            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;
            var allPMProjects = GetAllPMProjects();
            //ViewData["ProjectsList"];
            if (allPMProjects != null)
            {
                ViewData["ProjectsList"] = new SelectList(allPMProjects, "ProjectId", "Name");
            }
            else
            {
                Project fake = new Project { ProjectId = -1, Name = "" };
                List<Project> fakeProjectsList = new List<Project>();
                fakeProjectsList.Add(fake);
                ViewData["ProjectsList"] = new SelectList(fakeProjectsList, "ProjectId", "Name");
            }
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
        public IActionResult MyTasks()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);

            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];

            var allUsers = GetAllUsersAsObject();
            var currentUser = _providers.User.GetUserById(userId);
            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;
            var allPMProjects = GetAllPMProjects();
            ViewData["ProjectsList"] = null;
            if (allPMProjects.Count != 0)
            {
                ViewData["ProjectsList"] = new SelectList(allPMProjects, "ProjectId", "Name");
            }
            else
            {
                Project fake = new Project { ProjectId = -1, Name = "" };
                List<Project> fakeProjectsList = new List<Project>();
                fakeProjectsList.Add(fake);
                ViewData["ProjectsList"] = new SelectList(fakeProjectsList, "ProjectId", "Name");
            }
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

        public ActionResult GetRequestStats(int userId)
        {
            var newUserId = userId;
            if (userId == -1)
            {
                newUserId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            }
            UserEntitlements userEntitlements = _providers.User.GetUserEntitlementsById(newUserId);
            List<Request> requestsList = _providers.Request.GetAllApprovedRequests(newUserId);
            var usedAmountAnnual = 0;
            var usedAmountStudy = 0;
            var usedAmountWFH = 0;
            for (var i = 0; i < requestsList.Count(); i++)
            {
                if (requestsList[i].RequestTypeId == 1)
                {
                    if (requestsList[i].ApprovedMessage == "Rejected" || requestsList[i].ApprovedMessage == "Pending")
                    {
                        continue; //Don't count rejected requests
                    }
                    if ((requestsList[i].EndDate - requestsList[i].StartDate).Days == 0) {
                        if (requestsList[i].StartDate.ToString("dddd") != "Saturday" && requestsList[i].StartDate.ToString("dddd") != "Sunday")
                        {
                            usedAmountAnnual += 1;
                            continue;
                        }
                        else
                        {
                            continue;
                        }

                    }
                    // usedAmount += days;
                    for (var j = requestsList[i].StartDate; j <= requestsList[i].EndDate; j = j.AddDays(1))
                    {
                        var dayOfWeek = j.ToString("dddd");
                        if (dayOfWeek.Equals("Saturday") || dayOfWeek.Equals("Sunday")) //Don't count Weekend 
                        {
                            continue;
                        }
                        else
                        {
                            usedAmountAnnual += 1;
                        }
                    }
                }
                if (requestsList[i].RequestTypeId == 2)
                {
                    if (requestsList[i].ApprovedMessage == "Rejected" || requestsList[i].ApprovedMessage == "Pending")
                    {
                        continue; //Don't count rejected requests
                    }
                    if ((requestsList[i].EndDate - requestsList[i].StartDate).Days == 0) //one day's req 
                    {
                        if (requestsList[i].StartDate.ToString("dddd") != "Saturday" && requestsList[i].StartDate.ToString("dddd") != "Sunday")
                        {
                            usedAmountStudy += 1;
                            continue;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    // usedAmount += days;
                    for (var j = requestsList[i].StartDate; j <= requestsList[i].EndDate; j = j.AddDays(1))
                    {
                        var dayOfWeek = j.ToString("dddd");
                        if (dayOfWeek.Equals("Saturday") || dayOfWeek.Equals("Sunday")) //Don't count Weekend 
                        {
                            continue;
                        }
                        else
                        {
                            usedAmountStudy += 1;
                        }
                    }
                }
                if (requestsList[i].RequestTypeId == 3)
                {
                    if (requestsList[i].ApprovedMessage == "Rejected" || requestsList[i].ApprovedMessage == "Pending")
                    {
                        continue; //Don't count rejected requests
                    }
                    if ((requestsList[i].EndDate - requestsList[i].StartDate).Days == 0) //one day's req 
                    {
                        if (requestsList[i].StartDate.ToString("dddd") != "Saturday" && requestsList[i].StartDate.ToString("dddd") != "Sunday")
                        {
                            usedAmountWFH += 1;
                            continue;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    // usedAmount += days;
                    for (var j = requestsList[i].StartDate; j <= requestsList[i].EndDate; j = j.AddDays(1))
                    {
                        var dayOfWeek = j.ToString("dddd");
                        if (dayOfWeek.Equals("Saturday") || dayOfWeek.Equals("Sunday")) //Don't count Weekend 
                        {
                            continue;
                        }
                        else
                        {
                            usedAmountWFH += 1;
                        }
                    }
                }
            }
            RequestStats requestStats = new RequestStats
            {
                EntitlementAmount = userEntitlements.Amount,
                TotalUsed = usedAmountAnnual + usedAmountStudy + usedAmountWFH,
                AnnualUsed = usedAmountAnnual,
                StudyUsed = usedAmountStudy,
                WFHUsed = usedAmountWFH,
                UserId = newUserId

            };
            return Json(new { data = requestStats });

        }
        public List<Request> GetAllApprovedRequests()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            List<Request> allRequests = _providers.Request.GetAllApprovedRequests(userId);
            return allRequests;
        }

        public ActionResult GetAllApprovedRequestsAsJson(int userId)
        {
            var newUserId = userId;
            if (userId == -1)
            {
                newUserId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            }
            Console.WriteLine(newUserId);
            List<Request> allRequests = _providers.Request.GetAllApprovedRequests(newUserId);
            return Json(new { data = allRequests });
        }

        public List<List<Request>> GetFilteredRequests(string usersList)
        {
            var usersListFour = new List<Int32>();
            var usersListTwo = usersList.Replace("(", "").Replace(")", "");
            var usersListThree = usersListTwo.Split(",");

            //Convert a new list to map strings representing ints to ints 
            for (var i = 0; i < usersListThree.Count(); i++)
            {
                usersListFour.Add(Convert.ToInt32(usersListThree.ElementAt(i)));
            }
            List<List<Request>> allRequests = _providers.Request.GetFilteredRequests(usersListFour);
            return allRequests;
        }
        public ActionResult GetFilteredRequestsAsJson(string usersList)
        {
            List<List<Request>> allRequests = GetFilteredRequests(usersList);
            return Json(new { data = allRequests });
        }
        public ActionResult GetAllMeetings()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            List<Meeting> allMeetings = _providers.User.GetAllMeetings(userId);
            return Json(new { data = allMeetings });
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
            return _providers.User.GetUserEntitlements(userId);
        }
        public bool AddNewRequest(int userId, int requestTypeId, DateTime startDate, DateTime endDate, string requestDescription)
        {
            var success = _providers.Request.AddNewRequest(userId, requestTypeId, startDate, endDate, requestDescription);
            if (success)
            {
                var success2 = SendNewRequestEmail(userId, requestTypeId, startDate, endDate, requestDescription);
                if (success2)
                {
                    var success3 = SendNewRequestEmailToManagers(userId, requestTypeId, startDate, endDate, requestDescription);
                    if (success3) //do we need this nesting or is one success enough?
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        public bool AddNewMeeting(string title, string description, string priority, DateTime meetingDate, DateTime startTime, DateTime endTime, string attendees)
        {
            var userId = HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1];
            Console.WriteLine(description);
            Console.WriteLine(priority);
            var attendeesList = attendees.Replace("(", "").Replace(")", "");
            var attendeesListTwo = attendeesList.Split(",");
            var attendeesListThree = attendeesListTwo.Prepend<string>(userId);
            var attendeesListFour = new List<Int32>();

            var startTimeStr = startTime.ToString("hh:mm tt");
            var endTimeStr = endTime.ToString("hh:mm tt");

            //Convert a new list to map strings representing ints to ints 
            for (var i=0; i<attendeesListThree.Count(); i++)
            {
                attendeesListFour.Add(Convert.ToInt32(attendeesListThree.ElementAt(i)));
            }
            var success = _providers.User.AddNewMeeting(title, description, priority, meetingDate, startTimeStr, endTimeStr, attendeesListFour);
            if (success)
            {
                var hostUserId = attendeesListFour[0];
                for (var i=1; i<attendeesListFour.Count(); i++) //dont need to send meeting invite to the host 
                {
                    SendMeetingInviteEmail(attendeesListFour[i], hostUserId, title, description, priority, meetingDate, startTimeStr, endTimeStr);
                }   
                return true;
            }

            return false;
        }

        public bool DeleteRequest(int requestId)
        {
            return _providers.Request.DeleteRequest(requestId);
        }

        public bool AcceptMeeting(int meetingId)
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            var success = _providers.User.AcceptMeeting(userId, meetingId);
            if (success)
            {
                SendAcceptMeetingEmailToHost(userId, meetingId);
                return true;
            }
            return false;
        }

        public bool DeleteMeeting(int meetingId)
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);

            var meeting = _providers.User.GetMeetingById(meetingId);
            var meetingAttendees = meeting.Attendees;

            meetingAttendees = meetingAttendees.Replace("Tentative", "");
            var attendeesList = meetingAttendees.Split(",");
            var attendeesIds = _providers.User.GetMeetingAttendeesIdList(meetingId);

            if (meeting.HostUserId == userId) //if the host is the one cancelling the meeting, need to delete all meeting attendees of this meeting 
            {
                for (var i=1; i<attendeesIds.Count(); i++) //i = 1 because dont need to send to host 
                {
                    if (attendeesIds.ElementAt(i) == meeting.HostUserId)
                    {
                        continue;
                    }
                    SendCancelMeetingEmail(attendeesIds[i], meetingId);
                }
            }
            var success = _providers.User.DeleteMeeting(userId, meetingId);
            if (success)
            {
                SendRejectMeetingEmailToHost(userId, meetingId);
                return true;
            }
            return false;
        }
        public ActionResult TestError(int userId, int requestTypeId, DateTime startDate, DateTime endDate, string requestDescription)
        {
            Message toReturn = new Message { Id = 5, MessageStr = "Works ehre" };
            return Json(new { data = toReturn });
        }

        [HttpPost]
        public ActionResult ValidateRequest(int userId, int requestTypeId, DateTime startDate, DateTime endDate, string requestDescription)
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
            var requestsList = GetAllApprovedRequests();
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
            
            //Check if enough entitlements for the year, if the requestType is 'Annual'
            if (requestTypeId == 1)
            {
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
                            continue;
                       }
                       // usedAmount += days;
                       for (var j = requestsList[i].StartDate; j <= requestsList[i].EndDate; j = j.AddDays(1))
                       {
                            var dayOfWeek = j.ToString("dddd");
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
                if (usedAmount + requestAmount > totalAmount)
                {
                    toReturn.Id = 3;
                    toReturn.MessageStr = "Not enough entitlements.";
                    try
                    {
                        return Json(new { data = toReturn });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return Json(new { result = false, error = ex.Message });
                    }
                    //return Json(new { data = toReturn });
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
            var success = AddNewRequest(currentUserId, requestTypeId, startDate, endDate, requestDescription);
            if (success)
            {
                Console.WriteLine("About to send a message now success");
                toReturn.Id = 1;
                toReturn.MessageStr = "";
                var jsonobj = Json(new { data = toReturn });
                Console.WriteLine(jsonobj);
                //return Json(new { data = toReturn });
                return jsonobj;
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

        public int GetInstitutionId(int userId)
        {
            return _providers.User.GetInstitutionId(userId);

        }
        public ActionResult GetAllProjectsAsJson()
        {
            var projectList = GetAllProjects();
            return Json(new { data = projectList });
        }

        public List<Project> GetAllProjects() //all the projects that a user is a part of 
        {
            List<Project> allProjects = new List<Project>();
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);

            return _providers.User.GetAllProjects(userId);
        }
        public ActionResult GetAllProjectsStats() //all the projects that a user is a part of 
        {
            List<Project> allProjects = new List<Project>();
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);

            var projectList = _providers.User.GetAllProjectsStats(userId);
            return Json(new { data = projectList });
        }

        public ActionResult GetProjectsStatsById(int projectId) //all the projects that a user is a part of 
        {
            Project project = _providers.User.GetProjectsStatsById(projectId); ;
            return Json(new { data = project });
        }
        public bool GetProjectStatus(int projectId) //all the projects that a user is a part of 
        {
            bool projectStatus = _providers.User.GetProjectStatus(projectId); ;
            return projectStatus;
        }
        public List<Project> GetAllPMProjects() //all the projects for which user is project manager 
        {
            List<ProjectTask> allProjects = new List<ProjectTask>();
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            
            var projects = _providers.User.GetAllPMProjects(userId);
            return projects;
        }

        public ActionResult GetProjectPersonsById(int projectId)
        {
            List<User> projectPersons = _providers.User.GetProjectPersonsById(projectId);
            return Json(new { data = projectPersons });
        }

        public List<ProjectTask> GetAllPMTasks(int projectId)
        {
            List<ProjectTask> allProjects = new List<ProjectTask>();
            return _providers.User.GetAllPMTasks(projectId);
        }

        public List<ProjectTask> GetAllTasks(int userId)
        {
            return _providers.User.GetAllTasks(userId);
        }

        public ActionResult GetAllPMTasksAsJson(int projectId)
        {
            var tasksList = GetAllPMTasks(projectId);
            return Json(new { data = tasksList });
        }

        public ActionResult GetAllTasksAsJson(int projectId)
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            var tasksList = GetAllTasks(userId);
            return Json(new { data = tasksList });
        }

        public bool AddNewTask(int projectId, string task, DateTime deadline, int userId)
        {
            return _providers.User.AddNewTask(projectId, task, deadline, userId);
        }

        public bool DeleteTask(int taskId)
        {
            return _providers.User.DeleteTask(taskId);
        }
        public bool ToggleCompleteTask(int taskId, bool complete)
        {
            return _providers.User.ToggleCompleteTask(taskId, complete);
        }
        public bool UpdateTaskMethodPM(int updateTaskId, string updateTask, DateTime updateDeadline, int updateUser)
        {
            int currentUserId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            return _providers.User.UpdateTaskMethodPM(updateTaskId, updateTask, updateDeadline, updateUser);
        }
        public bool UpdateTaskMethod(int updateTaskId, string updateTask)
        {
            int currentUserId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            return _providers.User.UpdateTaskMethod(updateTaskId, updateTask);
        }

        public ActionResult GetTaskById(int taskId)
        {
            ProjectTask currentTask = _providers.User.GetTaskById(taskId);
            return Json(new { data = currentTask });
        }

        public ActionResult GetOrderedProjectPersonsById(int projectId)
        {
            List<User> projectPersons = _providers.User.GetOrderedProjectPersonsById(projectId);
            return Json(new { data = projectPersons });
        }

        public bool AllocateTask(int taskId, int userId)
        {
            return _providers.User.AllocateTask(taskId, userId);
        }

        public bool AddNewComment(int userId, int projectId, string comment)
        {
            return _providers.User.AddNewComment(userId, projectId, comment);
        }
        public bool ToggleCompleteProject(int projectId, bool complete)
        {
            Console.WriteLine(complete);
            return _providers.User.ToggleCompleteProject(projectId, complete);
        }
    }

}

