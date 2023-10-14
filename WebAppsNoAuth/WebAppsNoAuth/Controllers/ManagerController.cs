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

//The following two may need to be added to the provider file 
using Microsoft.ML;
using EmpSentimentModel;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
                WebAppsNoAuth.Models.Location fake = new WebAppsNoAuth.Models.Location { LocationId = -1, LocationValue = "", LocationTitle = "" };
                List<WebAppsNoAuth.Models.Location> fakeLocationsList = new List<WebAppsNoAuth.Models.Location>();
                fakeLocationsList.Add(fake);
                ViewData["UserLocationsList"] = new SelectList(fakeLocationsList, "LocationId", "LocationValue");

            }

            return View();
        }

        public IActionResult Projects()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Authenticated"] = "true";
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            User currentUser = _providers.User.GetUserById(userId);
            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;
            var allUsers = _providers.User.GetAllUsersAsObject(userId);
            ViewData["UsersListComplete"] = new SelectList(allUsers, "Id", "Name");

            var statusTypes = _providers.User.GetAllStatusTypes();
            ViewData["StatusList"] = new SelectList(statusTypes, "StatusTypeId", "StatusTypeName");
            var userLocationsList = GetAllUserLocations();
            if (userLocationsList.Count != 0)
            {
                ViewData["UserLocationsList"] = new SelectList(userLocationsList, "LocationId", "LocationValue");
            }
            else
            {
                WebAppsNoAuth.Models.Location fake = new WebAppsNoAuth.Models.Location { LocationId = -1, LocationValue = "", LocationTitle = "" };
                List<WebAppsNoAuth.Models.Location> fakeLocationsList = new List<WebAppsNoAuth.Models.Location>();
                fakeLocationsList.Add(fake);
                ViewData["UserLocationsList"] = new SelectList(fakeLocationsList, "LocationId", "LocationValue");

            }
            return View();
        }

        public IActionResult UpdateProject()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Authenticated"] = "true";
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            User currentUser = _providers.User.GetUserById(userId);
            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;
            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;
            var allUsers = _providers.User.GetAllUsersAsObject(userId);
            ViewData["UsersListComplete"] = new SelectList(allUsers, "Id", "Name");

            var statusTypes = _providers.User.GetAllStatusTypes();
            ViewData["StatusList"] = new SelectList(statusTypes, "StatusTypeId", "StatusTypeName");
            var userLocationsList = GetAllUserLocations();
            if (userLocationsList.Count != 0)
            {
                ViewData["UserLocationsList"] = new SelectList(userLocationsList, "LocationId", "LocationValue");
            }
            else
            {
                WebAppsNoAuth.Models.Location fake = new WebAppsNoAuth.Models.Location { LocationId = -1, LocationValue = "", LocationTitle = "" };
                List<WebAppsNoAuth.Models.Location> fakeLocationsList = new List<WebAppsNoAuth.Models.Location>();
                fakeLocationsList.Add(fake);
                ViewData["UserLocationsList"] = new SelectList(fakeLocationsList, "LocationId", "LocationValue");

            }
            return View();
        }

        public IActionResult EmployeeAnalytics()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Authenticated"] = "true";
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            User currentUser = _providers.User.GetUserById(userId);
            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;
            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;

            var managerUsers = GetAllManagerUsers(userId);
            ViewData["ManagerUsersList"] = new SelectList(managerUsers, "Id", "Name");

            var allUsers = _providers.User.GetAllUsersAsObject(userId);
            ViewData["UsersListComplete"] = new SelectList(allUsers, "Id", "Name");

            var statusTypes = _providers.User.GetAllStatusTypes();
            ViewData["StatusList"] = new SelectList(statusTypes, "StatusTypeId", "StatusTypeName");
            var userLocationsList = GetAllUserLocations();
            if (userLocationsList.Count != 0)
            {
                ViewData["UserLocationsList"] = new SelectList(userLocationsList, "LocationId", "LocationValue");
            }
            else
            {
                WebAppsNoAuth.Models.Location fake = new WebAppsNoAuth.Models.Location { LocationId = -1, LocationValue = "", LocationTitle = "" };
                List<WebAppsNoAuth.Models.Location> fakeLocationsList = new List<WebAppsNoAuth.Models.Location>();
                fakeLocationsList.Add(fake);
                ViewData["UserLocationsList"] = new SelectList(fakeLocationsList, "LocationId", "LocationValue");

            }
            return View();
        }
        public ActionResult GetAllUsersAsJson()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            IEnumerable<User> allUsers = _providers.User.GetAllUsersAsObject(userId);
            return Json(new { data = allUsers });
        }
        public ActionResult GetAllSkills()
        {
            IEnumerable<Skill> allSkills = _providers.Manager.GetAllSkills();
            return Json(new { data = allSkills });
        }
        //THESE METHODS NEED TO BE MOVED

        public List<WebAppsNoAuth.Models.Location> GetAllUserLocations()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            List<WebAppsNoAuth.Models.Location> allUsers = _providers.User.GetAllUserLocations(userId);
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

        public List<User> GetAllManagerUsers(int managerUserId) //get all the users who the manager manages 
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            List<User> allUsers = new List<User>();
            allUsers = _providers.Manager.GetAllManagerUsers(managerUserId);

            return allUsers;
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

        public ActionResult GetAllProjectsAsJson()
        {
            var projectList = GetAllProjects();
            return Json(new { data = projectList });
        }

        public List<WebAppsNoAuth.Models.Project> GetAllProjects()
        {
            List<WebAppsNoAuth.Models.Project> allProjects = new List<WebAppsNoAuth.Models.Project>();
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);

            return _providers.Manager.GetAllProjects(userId);
        }

        public bool AddNewProject(string projectName, string projectDes, int projectDiff, int projectManager, string projectPpl)
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]); //this is the managerUserId
            var pplList = projectPpl.Replace("(", "").Replace(")", "");
            var pplListTwo = pplList.Split(",");
            //var pplListThree = pplListTwo.Prepend<string>(userId);
            var pplListFour = new List<Int32>();

            //Convert a new list to map strings representing ints to ints 
            for (var i = 0; i < pplListTwo.Count(); i++)
            {
                pplListFour.Add(Convert.ToInt32(pplListTwo.ElementAt(i)));
            }
            var success = _providers.Manager.AddNewProject(projectName, projectDes, projectDiff, projectManager, pplListFour, DateTime.Now, userId);
            if (success)
            {
                for (var i = 0; i < pplListFour.Count(); i++) //dont need to send meeting invite to the host 
                {
                    SendProjectInviteEmail(pplListFour[i], projectManager, projectName);
                }
                return true;
            }

            return false;
        }
        public void SendProjectInviteEmail(int userId, int managerUserId, string projectName)
        {
            User user = _providers.User.GetUserById(userId);
            var userName = user.Name;
            var userEmail = user.Email;
            User manager = _providers.User.GetUserById(managerUserId);
            var managerName = manager.Name;

            _providers.Email.SendProjectInviteEmail(userId, userName, userEmail, managerUserId, managerName, projectName);
        }


        public void AddNewSkill(string skillName)
        {
            _providers.Manager.AddNewSkill(skillName);
        }

        public ActionResult GetProjectById(int projectId)
        {
            WebAppsNoAuth.Models.Project currentProject = _providers.Manager.GetProjectById(projectId);
            return Json(new { data = currentProject });
        }

        public List<User> GetAllUsersInProject(int projectId)
        {
            return _providers.Manager.GetAllUsersInProject(projectId);
        }

        public ActionResult GetAllUsersInProjectAsJson(int projectId)
        {
            List<User> usersInProject = GetAllUsersInProject(projectId);
            return Json(new { data = usersInProject });
        }

        public List<User> GetAllUsersNotInProject(int projectId)
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            var institutionId = _providers.User.GetInstitutionId(userId);
            return _providers.Manager.GetAllUsersNotInProject(projectId, userId, institutionId);
        }

        public ActionResult GetAllUsersNotInProjectAsJson(int projectId)
        {
            List<User> usersNotInProject = GetAllUsersNotInProject(projectId);
            return Json(new { data = usersNotInProject });
        }

        public bool AddUserToProject(int userId, int projectId) //shall we add institutionId to this table?
        {
            return _providers.Manager.AddUserToProject(userId, projectId);
        }

        public bool RemoveUserFromProject(int userId, int projectId)
        {
            return _providers.Manager.RemoveUserFromProject(userId, projectId);
        }

        public bool UpdateProjectMethod(int projectId, string projectName, string projectDescription, int projectDifficulty, int projectManager)
        {
            int currentUserId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            return _providers.Manager.UpdateProjectMethod(projectId, projectName, projectDescription, projectDifficulty, projectManager);
        }

        public bool DeleteProject(int projectId) //will have to update all tables so that instead of deleting you set Active to 0
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                return _providers.Manager.DeleteProject(projectId);
            }
        }

        public IOrderedEnumerable<KeyValuePair<string, float>> GetEmpCommentAnalysis(int userId)
        {
            return _providers.Manager.GetEmpCommentAnalysis(userId);
        }

    }
}

