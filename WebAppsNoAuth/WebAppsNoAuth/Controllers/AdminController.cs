using System;
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

        public AdminController(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
        {
            _webApps = webApps;
            _configuration = configuration;
        }

        //THESE METHODS NEED TO BE MOVED
        public bool IsUserAdmin(int userId)
        {
            bool isAdmin = false;
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT Admin FROM [Users] WHERE Id = @ID";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@ID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        if (dbReader.IsDBNull(0))
                        {
                            return isAdmin;
                        }
                        isAdmin = dbReader.GetBoolean(0);
                    }
                }
                return isAdmin;
            }
        }
        public bool IsUserManager(int userId)
        {
            bool isManager = false;
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT Manager FROM [Users] WHERE Id = @ID";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@ID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        if (dbReader.IsDBNull(0))
                        {
                            return isManager;
                        }
                        isManager = dbReader.GetBoolean(0);
                    }
                }
                return isManager;
            }
        }

        public int GetInstitutionId(int userId)
        {
            int institutionId = -1;
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT InstitutionID FROM [Users] WHERE Id = @USERID";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        if (dbReader.IsDBNull(0))
                        {
                            return institutionId;
                        }
                        institutionId = dbReader.GetInt32(0);
                    }
                }
                return institutionId;
            }
        }
        //THE ABOVE METHODS NEED TO BE MOVED TO THEIR OWN SEPARATE CLASS 
        // GET: /<controller>/
        public IActionResult UpdateUser()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            ViewData["Admin"] = IsUserAdmin(userId);
            ViewData["Manager"] = IsUserManager(userId);
            var managerList = GetAllManagers();
            ViewData["ManagerList"] = new SelectList(managerList, "Id", "Name");
            return View();
        }
        public IActionResult UpdateLocation()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            ViewData["Admin"] = IsUserAdmin(userId);
            ViewData["Manager"] = IsUserManager(userId);
            var managerList = GetAllManagers();
            ViewData["ManagerList"] = new SelectList(managerList, "Id", "Name");
            return View();
        }
        public IActionResult UpdateTeam()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            ViewData["Admin"] = IsUserAdmin(userId);
            ViewData["Manager"] = IsUserManager(userId);
            var locationList = GetAllLocationsAsObjectList();
            ViewData["LocationList"] = new SelectList(locationList, "LocationId", "LocationTitle");
            return View();
        }

        public IActionResult UpdateEntitlements()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            ViewData["Admin"] = IsUserAdmin(userId);
            ViewData["Manager"] = IsUserManager(userId);
            var locationList = GetAllLocationsAsObjectList();
            ViewData["LocationList"] = new SelectList(locationList, "LocationId", "LocationTitle");
            return View();
        }
        public ActionResult GetAllUsers()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            var institutionId = GetInstitutionId(userId);
            List<User> allUsers = new List<User>();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT U.[Id], U.[Name], U.[Email], U.[Password], U.[ContactNumber], U.[ManagerUserId], U2.[Name], U.[Admin], U.[Manager] FROM [Users] U " +
                                  "LEFT JOIN [Users] U2 ON U.[ManagerUserId] = U2.[Id] WHERE U.[InstitutionId] = @INSTITUTIONID"; //Left join to allow retrieval of users with null ManagerUserId 
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        User currentUser = new User();
                        currentUser.Id = dbReader.GetInt32(0);
                        currentUser.Name = dbReader.GetString(1);
                        currentUser.Email = dbReader.GetString(2);
                        currentUser.Password = dbReader.GetString(3);
                        currentUser.ContactNumber = dbReader.IsDBNull(4) ? "" : dbReader.GetString(4);
                        currentUser.ManagerUserId = dbReader.IsDBNull(5) ? -1 : dbReader.GetInt32(5);
                        currentUser.ManagerUserName = dbReader.IsDBNull(6) ? "" : dbReader.GetString(6);
                        currentUser.Admin = dbReader.IsDBNull(7) ? false : dbReader.GetBoolean(7);
                        currentUser.Manager = dbReader.IsDBNull(8) ? false : dbReader.GetBoolean(8);
                        allUsers.Add(currentUser);
                    }
                    connection.Close();
                }
            }
            return Json(new { data = allUsers });
        }

        public List<User> GetAllUsersInTeam(int teamId)
        {
            List<User> allUsers = new List<User>();
            if (teamId == -1)
            {
                User dummy = new User();
                dummy.Id = -1;
                allUsers.Add(dummy);
                return allUsers;
            }
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT U.[Id], U.[Name] FROM [Users] U JOIN [TeamUser] TU ON TU.[UserId] = U.[Id] WHERE TU.[TeamId] = @TEAMID"; //Left join to allow retrieval of users with null ManagerUserId 
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@TEAMID", teamId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        User currentUser = new User();
                        currentUser.Id = dbReader.GetInt32(0);
                        currentUser.Name = dbReader.GetString(1);
                        allUsers.Add(currentUser);
                    }
                    connection.Close();
                }
            }
            return allUsers;
        }

        public ActionResult GetAllUsersInTeamAsJson(int teamId)
        {
            List<User> usersInTeam = GetAllUsersInTeam(teamId);
            return Json(new { data = usersInTeam });
        }

        public List<User> GetAllUsersNotInTeam(int teamId)
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            int institutionId = GetInstitutionId(userId);
            List<int> usersInTeamId = GetAllUsersInTeam(teamId).Select(o => o.Id).ToList(); //take into consideration null lists
            String usersInTeamIdString = "";
            if (usersInTeamId != null)
            {
                usersInTeamIdString = "(" + String.Join(",", usersInTeamId) + ")"; //make this way work
            }
            else
            {
                usersInTeamIdString = "()";
            }
            List<User> allUsers = new List<User>();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT U.[Id], U.[Name] FROM [Users] U WHERE InstitutionId = @INSTITUTIONID"; 
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        User currentUser = new User();
                        currentUser.Id = dbReader.GetInt32(0);
                        if (usersInTeamId.Contains(currentUser.Id)) {
                           continue;
                        }
                        currentUser.Name = dbReader.GetString(1);
                        allUsers.Add(currentUser);
                    }
                    connection.Close();
                }
            }
            return allUsers;
        }

        public ActionResult GetAllUsersNotInTeamAsJson(int teamId)
        {
            List<User> usersNotInTeam = GetAllUsersNotInTeam(teamId);
            return Json(new { data = usersNotInTeam });
        }

        public ActionResult GetUserById(int userId)
        {
            User currentUser = new User();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT * FROM [Users] WHERE Id = @USERID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())

                    {
                        currentUser.Id = dbReader.GetInt32(0);
                        currentUser.Name = dbReader.GetString(1);
                        currentUser.Email = dbReader.GetString(2);
                        currentUser.Password = dbReader.GetString(3);
                        currentUser.ContactNumber = dbReader.IsDBNull(4) ? "" : dbReader.GetString(4);
                        currentUser.ManagerUserId = dbReader.IsDBNull(5) ? -1 : dbReader.GetInt32(5);
                        currentUser.Admin = dbReader.IsDBNull(6) ? false : dbReader.GetBoolean(6);
                        currentUser.Manager = dbReader.IsDBNull(7) ? false : dbReader.GetBoolean(7);
                    }
                    connection.Close();
                }
            }
            //  return allUsers;
            return Json(new { data = currentUser });
        }

        public ActionResult GetAllLocations()
        {
            var locationList = GetAllLocationsAsObjectList();
            return Json(new { data = locationList });
        }

        public List<Location> GetAllLocationsAsObjectList()
        {
            List<Location> allLocations = new List<Location>();
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            int institutionId = GetInstitutionId(userId);
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT * FROM [Location] WHERE InstitutionId = @INSTITUTIONID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        Location currentLocation = new Location();
                        currentLocation.LocationId = dbReader.GetInt32(0);
                        currentLocation.LocationName = dbReader.GetString(1);
                        currentLocation.LocationTitle = dbReader.GetString(2);
                        allLocations.Add(currentLocation);
                    }
                    connection.Close();
                }
            }
            return allLocations;
        }
        public ActionResult GetLocationById(int locationId)
        {
            Location currentLocation = new Location();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT * FROM [Location] WHERE LocationId = @LOCATIONID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@LOCATIONID", locationId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())

                    {
                        currentLocation.LocationId = dbReader.GetInt32(0);
                        currentLocation.LocationName = dbReader.GetString(1);
                        currentLocation.LocationTitle = dbReader.GetString(2);
                    }
                    connection.Close();
                }
            }
            return Json(new { data = currentLocation });
        }

        public ActionResult GetTeamById(int teamId)
        {
            Team currentTeam = new Team();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT * FROM [Team] WHERE TeamId = @TEAMID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@TEAMID", teamId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())

                    {
                        currentTeam.TeamId = dbReader.GetInt32(0);
                        currentTeam.TeamName = dbReader.GetString(1);
                        currentTeam.LocationId = dbReader.GetInt32(2);
                    }
                    connection.Close();
                }
            }
            return Json(new { data = currentTeam });
        }

        public ActionResult GetUserEntitlementsById(int userId)
        {
            UserEntitlements currentUserEntitlements = new UserEntitlements();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT UE.[UserId], UE.[Amount], UE.[Year], U.[Name] FROM [UserEntitlements] UE " +
                                  "JOIN [Users] U ON UE.[UserId] = U.[Id] WHERE UE.[UserId] = @USERID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())

                    {
                        currentUserEntitlements.UserId = dbReader.GetInt32(0);
                        currentUserEntitlements.Amount = dbReader.GetInt32(1);
                        currentUserEntitlements.Year = dbReader.GetInt32(2);
                        currentUserEntitlements.Name = dbReader.GetString(3);

                    }
                    connection.Close();
                }
            }
            return Json(new { data = currentUserEntitlements });
        }

        public List<Dropdown> GetAllManagers()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            int institutionId = GetInstitutionId(userId);
            List<Dropdown> allManagers = new List<Dropdown>();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT Id, Name FROM [Users] WHERE Manager = @TRUE AND InstitutionId = @INSTITUTIONID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@TRUE", true);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())

                    {
                        Dropdown currentManager = new Dropdown();
                        currentManager.Id = dbReader.GetInt32(0);
                        currentManager.Name = dbReader.GetString(1);
                        allManagers.Add(currentManager);
                    }
                    connection.Close();
                }
            }
            return allManagers;
        }

        public ActionResult GetAllLocationTeams(int locationId)
        {
            List<Team> allTeams = new List<Team>();
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            int institutionId = GetInstitutionId(userId);
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT * FROM [Team] WHERE LocationId = @LOCATIONID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@LOCATIONID", locationId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        Team currentTeam = new Team();
                        currentTeam.TeamId = dbReader.GetInt32(0);
                        currentTeam.TeamName = dbReader.GetString(1);
                        currentTeam.LocationId = dbReader.GetInt32(2);
                        allTeams.Add(currentTeam);
                    }
                    connection.Close();
                }
            }
            return Json(new { data = allTeams });
        }

        public ActionResult GetAllEntitlements()
        {
            List<UserEntitlements> allEntitlements = new List<UserEntitlements>();
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            int institutionId = GetInstitutionId(userId);
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT UE.[UserId], U.[Name], UE.[Amount] FROM [UserEntitlements] UE " +
                                  "JOIN [Users] U ON UE.[UserId] = U.[Id] WHERE U.[InstitutionId] = @INSTITUTIONID"; // based on year or location dropdown filter on userentitlements page?
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        UserEntitlements currentUserEntitlements = new UserEntitlements();
                        currentUserEntitlements.UserId = dbReader.GetInt32(0);
                        currentUserEntitlements.Name = dbReader.GetString(1);
                        currentUserEntitlements.Amount = dbReader.GetInt32(2);
                        allEntitlements.Add(currentUserEntitlements);
                    }
                    connection.Close();
                }
            }
            return Json(new { data = allEntitlements });
        }

        public bool AddNewUser(string name, string email, string password, string contactNumber, int managerUserId, string isAdmin, string isManager)
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            int institutionId = GetInstitutionId(userId);
            int addedUserId = -1;

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "INSERT INTO [Users] VALUES (@NAME, @EMAIL,@PASSWORD,@CONTACTNUMBER,@MANAGERUSERID,@ADMIN,@MANAGER,@INSTITUTIONID); " +
                                   "SELECT CAST(SCOPE_IDENTITY() AS INT)";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@NAME", name);
                command.Parameters.AddWithValue("@EMAIL", email);
                command.Parameters.AddWithValue("@PASSWORD", password); //will get rid of this
                command.Parameters.AddWithValue("@CONTACTNUMBER", contactNumber);
                command.Parameters.AddWithValue("@MANAGERUSERID", managerUserId);
                command.Parameters.AddWithValue("@ADMIN", Convert.ToBoolean(isAdmin)); //change so this val can be set
                command.Parameters.AddWithValue("@MANAGER", Convert.ToBoolean(isManager));
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId); //change so you can recieve the proper one from updateuser.cshtml
                // command.ExecuteNonQuery();

                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        addedUserId = dbReader.GetInt32(0);
                    }
                }

                connection.Close();

                var success = SetUserEntitlements(addedUserId);
                if (success) {
                    return true;
                }

            }
            return false;
        }

        public bool SetUserEntitlements(int userId)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "INSERT INTO [UserEntitlements] VALUES (@USERID, 0, @CURRENTYEAR)";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@CURRENTYEAR", DateTime.Today.Year); //change so you can recieve the proper one from updateuser.cshtml
                command.ExecuteNonQuery();

                connection.Close();
                return true;
            }
            return false;
        }
        public bool AddNewLocation(string locationName, string locationTitle)
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            int institutionId = GetInstitutionId(userId);

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "INSERT INTO [Location] VALUES (@LOCATIONNAME,@LOCATIONTITLE,@INSTITUTIONID)";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@LOCATIONNAME", locationName);
                command.Parameters.AddWithValue("@LOCATIONTITLE", locationTitle);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
                command.ExecuteNonQuery();

                connection.Close();

                return true;

            }

            return false;
        }
        public bool AddNewTeam(string teamName, int locationId)
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            int institutionId = GetInstitutionId(userId);

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "INSERT INTO [Team] VALUES (@TEAMNAME,@LOCATIONID)";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@TEAMNAME", teamName);
                command.Parameters.AddWithValue("@LOCATIONID", locationId);
                command.ExecuteNonQuery();

                connection.Close();

                return true;

            }

            return false;
        }

        public bool AddUserToTeam(int userId, int teamId)
        {
            int institutionId = GetInstitutionId(userId);

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "INSERT INTO [TeamUser] VALUES (@USERID,@TEAMID)";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@TEAMID", teamId);
                command.ExecuteNonQuery();

                connection.Close();

                return true;

            }

            return false;
        }

        public bool RemoveUserFromTeam(int userId, int teamId)
        {
            // int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            int institutionId = GetInstitutionId(userId);

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "DELETE FROM [TeamUser] WHERE UserId = @USERID AND TeamId = @TEAMID";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@TEAMID", teamId);
                command.ExecuteNonQuery();

                connection.Close();

                return true;

            }

            return false;
        }
        public string UpdateUserMethod(int userId, string name, string email, string password, string contactNumber, int managerUserId, string isAdmin, string isManager)
        {
            var wasManager = IsUserManager(userId);
            int currentUserId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "UPDATE [Users] SET Name = @NAME, Email = @EMAIL, Password = @PASSWORD,ContactNumber = @CONTACTNUMBER, " +
                                   "ManagerUserId = @MANAGERUSERID, Admin = @ADMIN, Manager = @MANAGER WHERE Id = @USERID";

                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@NAME", name);
                command.Parameters.AddWithValue("@EMAIL", email);
                command.Parameters.AddWithValue("@PASSWORD", password); //will get rid of this
                command.Parameters.AddWithValue("@CONTACTNUMBER", contactNumber);
                command.Parameters.AddWithValue("@MANAGERUSERID", managerUserId);
                command.Parameters.AddWithValue("@ADMIN", Convert.ToBoolean(isAdmin)); //change so this val can be set
                command.Parameters.AddWithValue("@MANAGER", Convert.ToBoolean(isManager));
                command.ExecuteNonQuery();

                connection.Close();
            }

            if (userId == currentUserId) //If the Admin has updated their own details, need to create a new ClaimsIdentity, hence redirect to login page
            {
                return "false";
            }
            if (wasManager != Convert.ToBoolean(isManager))
            {
                return "changedManager";
            }
            return "true";
        }

        public bool UpdateLocationMethod(int updateLocationId, string locationName, string locationTitle)
        {
            int currentUserId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "UPDATE [Location] SET LocationName = @LOCATIONNAME, LocationTitle = @LOCATIONTITLE WHERE LocationId = @LOCATIONID";

                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@LOCATIONNAME", locationName);
                command.Parameters.AddWithValue("@LOCATIONTITLE", locationTitle);
                command.Parameters.AddWithValue("@LOCATIONID", updateLocationId);
                command.ExecuteNonQuery();

                connection.Close();
            }
            return true;
        }

        public bool UpdateTeamMethod(int updateTeamId, string teamName)
        {
            int currentUserId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "UPDATE [Team] SET TeamName = @TEAMNAME WHERE TeamId = @TEAMID";

                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@TEAMNAME", teamName);
                command.Parameters.AddWithValue("@TEAMID", updateTeamId);
                command.ExecuteNonQuery();

                connection.Close();
            }
            return true;
        }

        public bool UpdateEntitlementsMethod(int userId, int year, int amount)
        {
            int currentUserId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "UPDATE [UserEntitlements] SET Amount = @AMOUNT WHERE UserId = @USERID AND Year = @YEAR";

                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@YEAR", year);
                command.Parameters.AddWithValue("@AMOUNT", amount);
                command.ExecuteNonQuery();

                connection.Close();
            }
            return true;
        }

        public bool DeleteUser(int userId) //will have to update all tables so that instead of deleting you set Active to 0
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "DELETE FROM [Users] WHERE Id = @USERID";

                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.ExecuteNonQuery();

                connection.Close();
            }
            return true;
        }

        public bool DeleteLocation(int locationId) //will have to update all tables so that instead of deleting you set Active to 0
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "DELETE FROM [Location] WHERE LocationId = @LOCATIONID";

                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@LOCATIONID", locationId);
                command.ExecuteNonQuery();

                connection.Close();
            }
            return true;
        }

        public bool DeleteTeam(int teamId) //will have to update all tables so that instead of deleting you set Active to 0
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "DELETE FROM [Team] WHERE TeamId = @TEAMID";

                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@TEAMID", teamId);
                command.ExecuteNonQuery();

                connection.Close();
            }
            return true;
        }
    }
}

