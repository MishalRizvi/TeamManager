﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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
    public class AdminController : Controller
    {
        private readonly WebAppsNoAuthDbContext _webApps;
        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection;

        private readonly ProviderController _providers;

        public AdminController(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
        {
            _webApps = webApps;
            _configuration = configuration;
            _providers = new ProviderController(webApps, configuration);
           // _connection = connection;

        }

        //THESE METHODS NEED TO BE MOVED
        public bool IsUserAdmin(int userId)
        {
            return _providers.User.IsUserAdmin(userId);
        }
        public bool IsUserManager(int userId)
        {
            return _providers.User.IsUserManager(userId);
        }

        public int GetInstitutionId(int userId)
        {
            return _providers.User.GetInstitutionId(userId);

        }
        //THE ABOVE METHODS NEED TO BE MOVED TO THEIR OWN SEPARATE CLASS 
        // GET: /<controller>/
        public IActionResult UpdateUser()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            User currentUser = _providers.User.GetUserById(userId);
            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;
            var managerList = GetAllManagers();
            ViewData["ManagerList"] = new SelectList(managerList, "Id", "Name");
            return View();
        }
        public IActionResult UpdateLocation()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            User currentUser = _providers.User.GetUserById(userId);
            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;
            var managerList = _providers.Manager.GetAllManagers(userId, currentUser.InstitutionId);
            ViewData["ManagerList"] = new SelectList(managerList, "Id", "Name");
            return View();
        }
        public IActionResult UpdateTeam()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            User currentUser = _providers.User.GetUserById(userId);
            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;
            var managerList = GetAllManagers();
            var locationList = GetAllLocations();
            ViewData["LocationList"] = new SelectList(locationList, "LocationId", "LocationTitle");
            return View();
        }

        public IActionResult UpdateEntitlements()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            User currentUser = _providers.User.GetUserById(userId);
            ViewData["Admin"] = currentUser.Admin;
            ViewData["Manager"] = currentUser.Manager;
            var locationList = GetAllLocations();
            ViewData["LocationList"] = new SelectList(locationList, "LocationId", "LocationTitle");
            return View();
        }
        public ActionResult GetAllUsers()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            List<User> allUsers = _providers.User.GetAllUsers(userId);

            return Json(new { data = allUsers });
        }

        public List<User> GetAllUsersInTeam(int teamId)
        {
            return _providers.User.GetAllUsersInTeam(teamId);
        }

        public ActionResult GetAllUsersInTeamAsJson(int teamId)
        {
            List<User> usersInTeam = GetAllUsersInTeam(teamId);
            return Json(new { data = usersInTeam });
        }

        public List<User> GetAllUsersNotInTeam(int teamId)
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            var institutionId = GetInstitutionId(userId);
            return _providers.User.GetAllUsersNotInTeam(teamId, userId, institutionId);
        }

        public ActionResult GetAllUsersNotInTeamAsJson(int teamId)
        {
            List<User> usersNotInTeam = GetAllUsersNotInTeam(teamId);
            return Json(new { data = usersNotInTeam });
        }

        public ActionResult GetUserById(int userId)
        {
            User currentUser = _providers.User.GetUserById(userId);
            return Json(new { data = currentUser });
        }

        public ActionResult GetAllLocationsAsJson()
        {
            var locationList = GetAllLocations();
            return Json(new { data = locationList });
        }

        public List<Location> GetAllLocations()
        {
            List<Location> allLocations = new List<Location>();
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            int institutionId = GetInstitutionId(userId);

            return _providers.Location.GetAllLocations(userId, institutionId);
        }
        public ActionResult GetLocationById(int locationId)
        {
            Location currentLocation = _providers.Location.GetLocationById(locationId);
            return Json(new { data = currentLocation });
        }

        public ActionResult GetTeamById(int teamId)
        {
            Team currentTeam = _providers.Team.GetTeamById(teamId);
            return Json(new { data = currentTeam });
        }

        public ActionResult GetUserEntitlementsById(int userId)
        {
            UserEntitlements currentUserEntitlements = _providers.User.GetUserEntitlementsById(userId);
            return Json(new { data = currentUserEntitlements });
        }

        public List<Dropdown> GetAllManagers()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            int institutionId = GetInstitutionId(userId);
            return _providers.Manager.GetAllManagers(userId, institutionId);
        }

        public ActionResult GetAllLocationTeams(int locationId)
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            int institutionId = GetInstitutionId(userId);
            List<Team> allTeams = _providers.Location.GetAllLocationTeams(locationId, userId, institutionId);
            return Json(new { data = allTeams });
        }

        public ActionResult GetAllEntitlements()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            int institutionId = GetInstitutionId(userId);
            List<UserEntitlements> allEntitlements = _providers.User.GetAllEntitlements(userId, institutionId);
            return Json(new { data = allEntitlements });
        }

        public bool AddNewUser(string name, string email, string password, string contactNumber, int managerUserId, string isAdmin, string isManager)
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            int institutionId = GetInstitutionId(userId);
            return _providers.User.AddNewUser(name, email, password, contactNumber, managerUserId, isAdmin, isManager, userId, institutionId);
        }
        public bool AddNewLocation(string locationName, string locationTitle)
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            int institutionId = GetInstitutionId(userId);

            return _providers.Location.AddNewLocation(locationName, locationTitle, userId, institutionId);
        }
        public bool AddNewTeam(string teamName, int locationId)
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            int institutionId = GetInstitutionId(userId);

            return _providers.Team.AddNewTeam(teamName, locationId, userId, institutionId);
        }

        public bool AddUserToTeam(int userId, int teamId) //shall we add institutionId to this table?
        {
            return _providers.Team.AddUserToTeam(userId, teamId);
        }

        public bool RemoveUserFromTeam(int userId, int teamId)
        {
            return _providers.Team.RemoveUserFromTeam(userId, teamId);
        }
        public string UpdateUserMethod(int userId, string name, string email, string password, string contactNumber, int managerUserId, string isAdmin, string isManager)
        {
            var wasManager = IsUserManager(userId);
            int currentUserId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);

            return _providers.User.UpdateUserMethod(userId, name, email, password, contactNumber, managerUserId, isAdmin, isManager, currentUserId, wasManager);
        }

        public bool UpdateLocationMethod(int updateLocationId, string locationName, string locationTitle)
        {
            int currentUserId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            return _providers.Location.UpdateLocationMethod(updateLocationId, locationTitle, locationTitle, currentUserId);
        }

        public bool UpdateTeamMethod(int updateTeamId, string teamName)
        {
            int currentUserId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            return _providers.Team.UpdateTeamMethod(updateTeamId, teamName, currentUserId);
;        }

        public bool UpdateEntitlementsMethod(int userId, int year, int amount)
        {
            return _providers.User.UpdateEntitlementsMethod(userId, year, amount);
        }

        public bool DeleteUser(int userId) //will have to update all tables so that instead of deleting you set Active to 0
        {
            return _providers.User.DeleteUser(userId);
        }

        public bool DeleteLocation(int locationId) //will have to update all tables so that instead of deleting you set Active to 0
        {
            return _providers.Location.DeleteLocation(locationId);
        }

        public bool DeleteTeam(int teamId) //will have to update all tables so that instead of deleting you set Active to 0
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                return _providers.Team.DeleteTeam(teamId);
            }
        }
    }
}

