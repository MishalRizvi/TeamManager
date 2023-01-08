using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WebAppsNoAuth.Models;
using System.Data.SqlClient;
using WebAppsNoAuth.Data;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAppsNoAuth.Controllers
{
    public class AccessController : Controller
    {
        private readonly WebAppsNoAuthDbContext _webApps;
        private readonly IConfiguration _configuration;

        public AccessController(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
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
        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
            {
                return Redirect("/Home/Index");
            }
            return View();
        }
        public IActionResult Relogin()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            ViewData["Admin"] = IsUserAdmin(userId);
            ViewData["Manager"] = IsUserManager(userId);
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        public IActionResult ConfirmEmail()
        {
            ViewData["Authenticated"] = "true";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User userIn)
        {
            var Id = -1;
            var name = "";
            var email = "";
            var password = "";
            var contactNumber = "";
            var managerUserId = -1;
            var admin = false;
            var manager = false;
            var institutionID = -1;

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "SELECT * FROM [Users] WHERE Email = @EMAIL";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@EMAIL", userIn.Email);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        if (dbReader.IsDBNull(0))
                        {
                            ViewData["ValidateMessage"] = "User cannot be found";
                            return View();
                        }
                        Id = dbReader.GetInt32(0);
                        name = dbReader.GetString(1);
                        email = dbReader.GetString(2);
                        password = dbReader.GetString(3);
                        contactNumber = dbReader.IsDBNull(4) ? "" : dbReader.GetString(4);
                        managerUserId = dbReader.IsDBNull(5) ? -1 : dbReader.GetInt32(5);
                        admin = dbReader.IsDBNull(6) ? false : dbReader.GetBoolean(6);
                        manager = dbReader.IsDBNull(7) ? false : dbReader.GetBoolean(7);
                        institutionID = dbReader.GetInt32(8);
                    }
                    if (password.Equals(userIn.Password))
                    {
                        List<Claim> claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.NameIdentifier, userIn.Email),
                            new Claim("ID", Id.ToString()),
                            new Claim("Username", name),
                        };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        AuthenticationProperties properties = new AuthenticationProperties()
                        {
                            AllowRefresh = true,
                            IsPersistent = true
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
                        ViewData["Authenticated"] = "true";
                        return Redirect("/Home/Index");
                    }
                }
            }
            ViewData["ValidateMessage"] = "User cannot be found";
            return View();
        }

        public IActionResult AddCompany(User userIn)
        {
            int institutionID = -1;
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "INSERT INTO [Company] VALUES (@INSTITUTION); SELECT CAST(SCOPE_IDENTITY() AS INT)";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@INSTITUTION", userIn.Institution);

                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        institutionID = dbReader.GetInt32(0);
                    }
                }
                connection.Close();
            }
            var success = AddUser2(userIn, institutionID);
            if (success)
            {
                return Redirect("/Home/Index");
            }
            return Redirect("/Shared/Error/");
        }

        // [HttpPost]
        public bool AddUser2(User userIn, int institutionID)
        {
            var addedUserId = -1;
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "INSERT INTO [Users] VALUES (@NAME, @EMAIL,@PASSWORD,NULL,NULL,@ADMIN,@FALSE,@INSTITUTIONID); " +
                                        "SELECT CAST(SCOPE_IDENTITY() AS INT)";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@NAME", userIn.Name);
                command.Parameters.AddWithValue("@EMAIL", userIn.Email);
                command.Parameters.AddWithValue("@PASSWORD", userIn.Password);
                command.Parameters.AddWithValue("@ADMIN", true);
                command.Parameters.AddWithValue("@FALSE", false);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionID);

                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        addedUserId = dbReader.GetInt32(0);
                    }
                }
                connection.Close();

                var success = SetAdminEntitlements(addedUserId);

                if (success)
                {
                    return true;
                }
            }

            return false;
        }

        public bool SetAdminEntitlements(int userId)
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
    }
}

