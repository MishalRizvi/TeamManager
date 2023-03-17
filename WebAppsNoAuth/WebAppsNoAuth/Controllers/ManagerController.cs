using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebAppsNoAuth.Data;
using WebAppsNoAuth.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAppsNoAuth.Controllers
{
    public class ManagerController : Controller
    {
        private readonly WebAppsNoAuthDbContext _webApps;
        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection;

        private readonly ProviderController _providers;

        public ManagerController(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
        {
            _webApps = webApps;
            _configuration = configuration;
            _providers = new ProviderController(webApps, configuration);
        }

        // GET: /<controller>/
        public IActionResult ApproveRequest()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Authenticated"] = "true";
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            User currentUser = _providers.User.GetUserById(userId);
            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;

            var statusTypes = _providers.User.GetAllStatusTypes();
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

        //THESE METHODS NEED TO BE MOVED

        public List<Location> GetAllUserLocations()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            List<Location> allUsers = _providers.User.GetAllUserLocations(userId);

            return allUsers;
        }
        //public bool IsUserAdmin(int userId)
        //{
        //    return _providers.User.IsUserAdmin(userId);
        //}
        //public bool IsUserManager(int userId)
        //{
        //    return _providers.User.IsUserManager(userId);

        //}

        public string GetUserName(int userId)
        {
            return _providers.User.GetUserName(userId);

        }

        //Manager getting another users requests
        public Request GetRequestById(int requestId)
        {
            return _providers.Request.GetRequestById(requestId);
        }
        public bool SendApproveRequestEmail(int requestId)
        {
            EmailTemplate emailTemplate = new EmailTemplate();
            emailTemplate.Subject = "Request Approved";

            var request = GetRequestById(requestId);
            var userId = request.UserId;
            var requestTypeName = request.RequestTypeName;
            var requestStartDate = request.StartDateStr;
            var requestEndDate = request.EndDateStr;
            User currentUser = _providers.User.GetUserById(userId);
            var userName = currentUser.Name;
            var userEmail = currentUser.Email;
            var managerId = currentUser.ManagerUserId;

            var managerName = GetUserName(managerId);

            return _providers.Email.SendApproveRequestEmail(userId, requestTypeName, requestStartDate, requestEndDate, userName, userEmail, managerId, managerName);
        }

        public bool SendRejectRequestEmail(int requestId)
        {
            EmailTemplate emailTemplate = new EmailTemplate();
            emailTemplate.Subject = "Request Rejected";

            var request = GetRequestById(requestId);
            var userId = request.UserId;
            var requestTypeName = request.RequestTypeName;
            var requestStartDate = request.StartDateStr;
            var requestEndDate = request.EndDateStr;
            User currentUser = _providers.User.GetUserById(userId);
            var userName = currentUser.Name;
            var userEmail = currentUser.Email;
            var managerId = currentUser.ManagerUserId;

            var managerName = GetUserName(managerId);

            return _providers.Email.SendRejectRequestEmail(userId, requestTypeName, requestStartDate, requestEndDate, userName, userEmail, managerId, managerName);
        }

        public ActionResult GetAllManagerRequests()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]); //this is the managerUserId
            List<Request> allRequests = _providers.Request.GetAllManagerRequests(userId);
            return Json(new { data = allRequests });
        }

        public bool ApproveRequestMethod(int requestId)
        {
            var success = _providers.Request.ApproveRequestMethod(requestId);
            if (success)
            {
                var success2 = SendApproveRequestEmail(requestId);
                if (success2)
                {
                    return true;
                }
            }
            return false;

        }
        

        public bool RejectRequestMethod(int requestId)
        {
            var success = _providers.Request.RejectRequestMethod(requestId);
            if (success)
            {
                var success2 = SendRejectRequestEmail(requestId);
                if (success2)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

