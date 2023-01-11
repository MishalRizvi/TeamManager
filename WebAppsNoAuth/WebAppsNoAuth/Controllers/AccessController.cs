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
        private readonly SqlConnection _connection;

        private readonly ProviderController _providers;

        public AccessController(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
        {
            _webApps = webApps;
            _configuration = configuration;
            _connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb"));
            _providers = new ProviderController(webApps, configuration);
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

            using (_connection)
            {
                _connection.Open();

                var queryString = "SELECT * FROM [Users] WHERE Email = @EMAIL";
                SqlCommand command = new SqlCommand(queryString, _connection);
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
            using (_connection)
            {
                _connection.Open();

                var queryString = "INSERT INTO [Company] VALUES (@INSTITUTION); SELECT CAST(SCOPE_IDENTITY() AS INT)";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@INSTITUTION", userIn.Institution);

                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        institutionID = dbReader.GetInt32(0);
                    }
                }
                _connection.Close();
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
            return _providers.User.AddUser2(userIn, institutionID);
        }
    }
}

