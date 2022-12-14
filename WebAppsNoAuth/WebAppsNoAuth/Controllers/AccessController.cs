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
        // GET: /<controller>/
        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
            {
                return Redirect("/Home/Index");
            }
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
            var admin = false;
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
                        admin = dbReader.GetBoolean(4);
                        institutionID = dbReader.GetInt32(5);
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

                var queryString = @"INSERT INTO [Company] VALUES (@INSTITUTION); SELECT CAST(SCOPE_IDENTITY() AS INT)";
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
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO [Users] VALUES (@NAME, @EMAIL,@PASSWORD,@ADMIN,@INSTITUTIONID)";
                command.Parameters.AddWithValue("@NAME", userIn.Name);
                command.Parameters.AddWithValue("@EMAIL", userIn.Email);
                command.Parameters.AddWithValue("@PASSWORD", userIn.Password);
                command.Parameters.AddWithValue("@ADMIN", true);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionID);
                command.ExecuteNonQuery();

                connection.Close();

                return true;

            }

            return false;
        }
    }
}

