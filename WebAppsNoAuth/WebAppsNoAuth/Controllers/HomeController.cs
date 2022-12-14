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
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAppsNoAuth.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly WebAppsNoAuthDbContext _webApps;
        private readonly IConfiguration _configuration;

        public HomeController(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
        {
            _webApps = webApps;
            _configuration = configuration;
        }
        // GET: /<controller>/
        public IActionResult Index()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            ViewData["Admin"] = IsUserAdmin(userId);
            return View();
        }

        public IActionResult Privacy()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            ViewData["Admin"] = IsUserAdmin(userId);
            return View();
        }

        public async Task<IActionResult> LogOut()
        {
            //Add additional logic here to confirm if user wants to log out
            ViewData["Authenticated"] = "true"; //?
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            ViewData["Admin"] = IsUserAdmin(userId);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Access");
        }

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}

