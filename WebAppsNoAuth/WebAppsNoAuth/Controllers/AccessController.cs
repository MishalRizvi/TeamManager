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
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography;

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

        public List<Location> GetAllUserLocations()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            List<Location> allUsers = _providers.User.GetAllUserLocations(userId);

            return allUsers;
        }
        //THE ABOVE METHODS NEED TO BE MOVED TO THEIR OWN SEPARATE CLASS 
        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
            {
                Debug.WriteLine("REDIRECTING TO INDEX");
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
            var storedPassword = "";
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
                        storedPassword = dbReader.GetString(3);
                        contactNumber = dbReader.IsDBNull(4) ? "" : dbReader.GetString(4);
                        managerUserId = dbReader.IsDBNull(5) ? -1 : dbReader.GetInt32(5);
                        admin = dbReader.IsDBNull(6) ? false : dbReader.GetBoolean(6);
                        manager = dbReader.IsDBNull(7) ? false : dbReader.GetBoolean(7);
                        institutionID = dbReader.GetInt32(8);
                    }
                    if (VerifyPassword(userIn.Password, storedPassword))
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

        public static bool IsHashSupported(string hashString)
        {
            return hashString.Contains("$MYHASH$V1$");
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (!IsHashSupported(hashedPassword))
            {
                // throw new NotSupportedException("The hashtype is not supported.");
                return true; //for now for dealing with already stored passwords that are not hashed
            }
            var splittedHashString = hashedPassword.Replace("$MYHASH$V1$", "").Split('$');
            var iterations = int.Parse(splittedHashString[0]);
            var base64Hash = splittedHashString[1];
            var hashBytes = Convert.FromBase64String(base64Hash);
            var saltSize = 16;
            var hashSize = 10;
            var salt = new byte[saltSize];
            Array.Copy(hashBytes, 0, salt, 0, saltSize);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            byte[] hash = pbkdf2.GetBytes(hashSize);
            for (int i = 0; i < hashSize; i++)
            {
                if (hashBytes[i + saltSize] != hash[i])
                {
                    return false;
                }
            }
            return true;

        }

        public bool VerifyPasswordOnReset(string password)
        {
            var userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            User currentUser = _providers.User.GetUserById(userId);
            var hashedPassword = currentUser.Password;
            return VerifyPassword(password, hashedPassword);

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
            return _providers.User.AddUser2(userIn, institutionID); //This is for adding a new user who creates an institution 
        }
    }
}

