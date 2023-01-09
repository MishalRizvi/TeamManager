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
//using MailKit.Net.Smtp;
using Org.BouncyCastle.Crypto.Macs;
using System.Net.Mail;
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
                connection.Close();
            }
            return isAdmin;
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
                connection.Close();
            }
            return isManager;
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
                connection.Close();
            }
            return institutionId;
        }
        public string GetUserName(int userId)
        {
            var userName = "";
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT Name FROM [Users] WHERE Id = @USERID";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        if (dbReader.IsDBNull(0))
                        {
                            return userName;
                        }
                        userName = dbReader.GetString(0);
                    }
                }
                connection.Close();
            }
            return userName;
        }
        public string GetUserEmail(int userId)
        {
            var userEmail = "";
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT Email FROM [Users] WHERE Id = @USERID";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        if (dbReader.IsDBNull(0))
                        {
                            return userEmail;
                        }
                        userEmail = dbReader.GetString(0);
                    }
                }
                connection.Close();
            }
            return userEmail;
        }

        public int GetManager(int userId)
        {
            var managerUserId = -1;
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT ManagerUserId FROM [Users] WHERE Id = @USERID";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        if (dbReader.IsDBNull(0))
                        {
                            return managerUserId;
                        }
                        managerUserId = dbReader.GetInt32(0);
                    }
                    connection.Close();
                }
            }
            return managerUserId;
        }
        public bool SendNewRequestEmail(int userId, int requestTypeId, DateTime startDate, DateTime endDate)
        {
            EmailTemplate emailTemplate = new EmailTemplate();
            emailTemplate.Subject = "New Request";
            var userName = GetUserName(userId);
            var userEmail = GetUserEmail(userId);
            var requestTypeName = "";

            //Direct mapping of request type id to string done here:
            if (requestTypeId == 1)
            {
                requestTypeName = "Annual";
            }
            if (requestTypeId == 2)
            {
                requestTypeName = "Study";
            }
            if (requestTypeId == 3)
            {
                requestTypeName = "WFH";
            }
            emailTemplate.Body = "Hi " + userName + "," + Environment.NewLine +
                                 "You have submitted a new request." + Environment.NewLine +
                                 "Request Type: " + requestTypeName + Environment.NewLine +
                                 "Start Date: " + startDate.ToString("dd/MM/yyyy") + Environment.NewLine +
                                 "End Date: " + endDate.ToString("dd/MM/yyyy") + Environment.NewLine;
            emailTemplate.To = userEmail;
            emailTemplate.ToName = userName;
            var success = SendEmail(emailTemplate);
            if (success)
            {
                return true;
            }
            return false;
        }

        public bool SendNewRequestEmailToManagers(int userId, int requestTypeId, DateTime startDate, DateTime endDate)
        {
            EmailTemplate emailTemplate = new EmailTemplate();
            emailTemplate.Subject = "New Request";
            var userName = GetUserName(userId);
            var managerId = GetManager(userId);
            if (managerId == -1)
            {
                return true;
            }
            var managerName = GetUserName(managerId);
            var managerEmail = GetUserEmail(managerId);

            var requestTypeName = "";

            //Direct mapping of request type id to string done here:
            if (requestTypeId == 1)
            {
                requestTypeName = "Annual";
            }
            if (requestTypeId == 2)
            {
                requestTypeName = "Study";
            }
            if (requestTypeId == 3)
            {
                requestTypeName = "WFH";
            }
            emailTemplate.Body = "Hi " + managerName + "," + Environment.NewLine +
                                 userName + " has submitted a new request." + Environment.NewLine +
                                 "Request Type: " + requestTypeName + Environment.NewLine +
                                 "Start Date: " + startDate.ToString("dd/MM/yyyy") + Environment.NewLine +
                                 "End Date: " + endDate.ToString("dd/MM/yyyy") + Environment.NewLine;
            emailTemplate.To = managerEmail;
            emailTemplate.ToName = managerName;
            var success = SendEmail(emailTemplate);
            if (success)
            {
                return true;
            }
            return false;
        }

        public bool SendEmail(EmailTemplate emailTemplate)
        {
            //var email = new MimeMessage();
            //email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailConfiguration")["From"]));
            //Debug.WriteLine(_configuration.GetSection("EmailConfiguration")["From"]);
            //email.To.Add(MailboxAddress.Parse(_configuration.GetSection("EmailConfiguration")["From"])); //change 
            //email.Subject = "subjectTest";
            //email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };
            //using var smtp = new SmtpClient();
            //smtp.Connect("servicesappforyou@gmail.com", 587, SecureSocketOptions.StartTls);
            //smtp.Authenticate("servicesappforyou@gmail.com", "ServicesApp111");
            //smtp.Send(email);
            //smtp.Disconnect(true);
       

            try
            {
                MailMessage m = new MailMessage();
                SmtpClient sc = new SmtpClient();
                m.From = new MailAddress(_configuration.GetSection("EmailConfiguration")["From"], "Services App For You");
                Debug.WriteLine(emailTemplate.To);
                Debug.WriteLine(emailTemplate.ToName);
                m.To.Add(new MailAddress(emailTemplate.To, emailTemplate.ToName));
                m.Subject = emailTemplate.Subject;
                m.Body = emailTemplate.Body;
                m.IsBodyHtml = false;
                sc.Host = "smtp.gmail.com";
                sc.Port = 587;
                sc.Credentials = new System.Net.NetworkCredential(_configuration.GetSection("EmailConfiguration")["From"], (_configuration.GetSection("EmailConfiguration")["Password"])); //change
                sc.EnableSsl = true; // runtime encrypt the SMTP communications using SSL
                sc.Send(m);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Cannot send email");
                Debug.WriteLine(e);
            }

            return false;
        }

        public List<User> GetAllUsersAsObject()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            var institutionId = GetInstitutionId(userId);
            List<User> allUsers = new List<User>();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT U.[Id], U.[Name] FROM Users U WHERE Id = @USERID " +
                                  "UNION SELECT U.[Id], U.[Name] FROM Users U WHERE InstitutionId = @INSTITUTIONID"; //Left join to allow retrieval of users with null ManagerUserId 
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
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

        //THE ABOVE METHODS NEED TO BE MOVED TO THEIR OWN SEPARATE CLASS 
        // GET: /<controller>/
        public IActionResult Index()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            ViewData["Admin"] = IsUserAdmin(userId);
            ViewData["Manager"] = IsUserManager(userId);
            return View();
        }

        public IActionResult Holidays()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            ViewData["Admin"] = IsUserAdmin(userId);
            ViewData["Manager"] = IsUserManager(userId);
            var managerUsers = GetAllManagerUsers(userId);
            ViewData["ManagerUsersList"] = new SelectList(managerUsers, "Id", "Name");
            var requestTypes = GetAllRequestTypes();
            ViewData["RequestTypeList"] = new SelectList(requestTypes, "RequestTypeId", "RequestTypeName");

            return View();
        }

        public IActionResult Calendar()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            ViewData["Admin"] = IsUserAdmin(userId);
            ViewData["Manager"] = IsUserManager(userId);
           // var managerUsers = GetAllManagerUsers(userId);
          //  ViewData["ManagerUsersList"] = new SelectList(managerUsers, "Id", "Name");
            var requestTypes = GetAllRequestTypes();
            ViewData["RequestTypeList"] = new SelectList(requestTypes, "RequestTypeId", "RequestTypeName");

            var allUsers = GetAllUsersAsObject();
            ViewData["UsersList"] = new SelectList(allUsers, "Id", "Name");

            return View();
        }
        public IActionResult Privacy()
        {
            ViewData["Authenticated"] = "true";
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            ViewData["Admin"] = IsUserAdmin(userId);
            ViewData["Manager"] = IsUserManager(userId);
            return View();
        }

        public async Task<IActionResult> LogOut()
        {
            //Add additional logic here to confirm if user wants to log out
            ViewData["Authenticated"] = "true"; //?
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            ViewData["Admin"] = IsUserAdmin(userId);
            ViewData["Manager"] = IsUserManager(userId);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Access");
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
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT U.[Id], U.[Name] FROM Users U WHERE U.[Id] = @USERID " +
                                   "UNION SELECT U.[Id], U.[Name] FROM Users U WHERE U.[ManagerUserId] = @MANAGERUSERID";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@MANAGERUSERID", managerUserId);
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

        public List<RequestType> GetAllRequestTypes()
        {
            List<RequestType> allRequests = new List<RequestType>();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT * FROM RequestType";
                SqlCommand command = new SqlCommand(queryString, connection);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        RequestType requestType = new RequestType();
                        requestType.RequestTypeId = dbReader.GetInt32(0);
                        requestType.RequestTypeName = dbReader.GetString(1);
                        allRequests.Add(requestType);
                    }
                    connection.Close();
                }
            }
            return allRequests;
        }

        public ActionResult GetAllRequests()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            List<Request> allRequests = new List<Request>();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT R.[RequestId], R.[RequestTypeId], R.[UserId], R.[StartDate], R.[EndDate], RT.[RequestTypeName], R.[Approved] FROM Request R " +
                                  "JOIN RequestType RT ON R.[RequestTypeId] = RT.[RequestTypeId] WHERE R.[UserId] = @USERID";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        Request currentRequest = new Request();
                        currentRequest.RequestId = dbReader.GetInt32(0);
                        currentRequest.RequestTypeId = dbReader.GetInt32(1);
                        currentRequest.UserId = dbReader.GetInt32(2);
                        currentRequest.StartDate = dbReader.GetDateTime(3);
                        currentRequest.StartDateStr = currentRequest.StartDate.ToString("dd/MM/yyyy");
                        currentRequest.EndDate = dbReader.GetDateTime(4);
                        currentRequest.EndDateStr = currentRequest.EndDate.ToString("dd/MM/yyyy");
                        currentRequest.RequestTypeName = dbReader.GetString(5);
                        if (dbReader.IsDBNull(6))
                        {
                            currentRequest.ApprovedMessage = "Pending";
                        }
                        else
                        {
                            var approved = dbReader.GetBoolean(6);
                            if (approved)
                            {
                                currentRequest.ApprovedMessage = "Approved";
                            }
                            else if (approved == false)
                            {
                                currentRequest.ApprovedMessage = "Rejected";
                            }
                        }
                        allRequests.Add(currentRequest);
                    }
                    connection.Close();
                }
            }
            return Json(new { data = allRequests });
        }
        public ActionResult GetAllApprovedRequests(int userId)
        {
           // int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            List<Request> allRequests = new List<Request>();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT R.[RequestId], R.[RequestTypeId], R.[UserId], R.[StartDate], R.[EndDate], RT.[RequestTypeName], R.[Approved] FROM Request R " +
                                  "JOIN RequestType RT ON R.[RequestTypeId] = RT.[RequestTypeId] WHERE R.[UserId] = @USERID AND R.[Approved] = @TRUE";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@TRUE", true);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        Request currentRequest = new Request();
                        currentRequest.RequestId = dbReader.GetInt32(0);
                        currentRequest.RequestTypeId = dbReader.GetInt32(1);
                        currentRequest.UserId = dbReader.GetInt32(2);
                        currentRequest.StartDate = dbReader.GetDateTime(3);
                        currentRequest.StartDateStr = currentRequest.StartDate.ToString("dd/MM/yyyy");
                        currentRequest.EndDate = dbReader.GetDateTime(4);
                        currentRequest.EndDateStr = currentRequest.EndDate.ToString("dd/MM/yyyy");
                        currentRequest.RequestTypeName = dbReader.GetString(5);
                        if (dbReader.IsDBNull(6))
                        {
                            currentRequest.ApprovedMessage = "Pending";
                        }
                        else
                        {
                            var approved = dbReader.GetBoolean(6);
                            if (approved)
                            {
                                currentRequest.ApprovedMessage = "Approved";
                            }
                            else if (approved == false)
                            {
                                currentRequest.ApprovedMessage = "Rejected";
                            }
                        }
                        allRequests.Add(currentRequest);
                    }
                    connection.Close();
                }
            }
            return Json(new { data = allRequests });
        }
        //Manager getting another users requests
        public ActionResult GetUserRequests(int userId)
        {
            List<Request> allRequests = new List<Request>();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT R.[RequestId], R.[RequestTypeId], R.[UserId], R.[StartDate], R.[EndDate], RT.[RequestTypeName], R.[Approved] FROM Request R " +
                                  "JOIN RequestType RT ON R.[RequestTypeId] = RT.[RequestTypeId] WHERE R.[UserId] = @USERID";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        Request currentRequest = new Request();
                        currentRequest.RequestId = dbReader.GetInt32(0);
                        currentRequest.RequestTypeId = dbReader.GetInt32(1);
                        currentRequest.UserId = dbReader.GetInt32(2);
                        currentRequest.StartDate = dbReader.GetDateTime(3);
                        currentRequest.StartDateStr = currentRequest.StartDate.ToString("dd/MM/yyyy");
                        currentRequest.EndDate = dbReader.GetDateTime(4);
                        currentRequest.EndDateStr = currentRequest.EndDate.ToString("dd/MM/yyyy");
                        currentRequest.RequestTypeName = dbReader.GetString(5);
                        if (dbReader.IsDBNull(6))
                        {
                            currentRequest.ApprovedMessage = "Pending";
                        }
                        else
                        {
                            var approved = dbReader.GetBoolean(6);
                            if (approved)
                            {
                                currentRequest.ApprovedMessage = "Approved";
                            }
                            else if (approved == false)
                            {
                                currentRequest.ApprovedMessage = "Rejected";
                            }
                        }
                        allRequests.Add(currentRequest);
                    }
                    connection.Close();
                }
            }
            return Json(new { data = allRequests });
        }

        public List<Request> GetUserRequestsAsObject(int userId)
        {
            List<Request> allRequests = new List<Request>();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT R.[RequestId], R.[RequestTypeId], R.[UserId], R.[StartDate], R.[EndDate], RT.[RequestTypeName], R.[Approved] FROM Request R " +
                                  "JOIN RequestType RT ON R.[RequestTypeId] = RT.[RequestTypeId] WHERE R.[UserId] = @USERID";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        Request currentRequest = new Request();
                        currentRequest.RequestId = dbReader.GetInt32(0);
                        currentRequest.RequestTypeId = dbReader.GetInt32(1);
                        currentRequest.UserId = dbReader.GetInt32(2);
                        currentRequest.StartDate = dbReader.GetDateTime(3);
                        currentRequest.StartDateStr = currentRequest.StartDate.ToString("dd/MM/yyyy");
                        currentRequest.EndDate = dbReader.GetDateTime(4);
                        currentRequest.EndDateStr = currentRequest.EndDate.ToString("dd/MM/yyyy");
                        currentRequest.RequestTypeName = dbReader.GetString(5);
                        if (dbReader.IsDBNull(6))
                        {
                            currentRequest.ApprovedMessage = "Pending";
                        }
                        else
                        {
                            var approved = dbReader.GetBoolean(6);
                            if (approved)
                            {
                                currentRequest.ApprovedMessage = "Approved";
                            }
                            else if (approved == false)
                            {
                                currentRequest.ApprovedMessage = "Rejected";
                            }
                        }
                        allRequests.Add(currentRequest);
                    }
                    connection.Close();
                }
            }
            return allRequests;
        }
        public int GetUserEntitlements(int userId)
        {
            var amount = -1;
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT Amount FROM [UserEntitlements] WHERE UserId = @USERID AND Year = @CURRENTYEAR";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@CURRENTYEAR", DateTime.Today.Year);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        amount = dbReader.GetInt32(0);
                    }
                    connection.Close();
                }
            }
            return amount;
        }
        public bool AddNewRequest(int userId, int requestTypeId, DateTime startDate, DateTime endDate)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "INSERT INTO [Request] VALUES (@REQUESTTYPEID,@USERID,@STARTDATE,@ENDDATE,NULL)"; //what if manager is doing this on behalf?
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@REQUESTTYPEID", requestTypeId);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@STARTDATE", startDate);
                command.Parameters.AddWithValue("@ENDDATE", endDate);
                command.ExecuteNonQuery();

                connection.Close();

                var success = SendNewRequestEmail(userId, requestTypeId, startDate, endDate);
                if (success)
                {
                    var success2 = SendNewRequestEmailToManagers(userId, requestTypeId, startDate, endDate);
                    if (success2) //do we need this nesting or is one success enough?
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool DeleteRequest(int requestId)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "DELETE FROM [Request] WHERE RequestId = @REQUESTID"; //what if manager is doing this on behalf?
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@REQUESTID", requestId);
                command.ExecuteNonQuery();

                connection.Close();

                return true;

            }

            return false;
        }

        public ActionResult ValidateRequest(int userId, int requestTypeId, DateTime startDate, DateTime endDate)
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
            var requestsList = GetUserRequestsAsObject(currentUserId);
            var usedAmount = 0;
            var requestAmount = (endDate - startDate).Days + 1;

            //Check if enough entitlements for the year, if the requestType is 'Annual'
            if (requestTypeId == 1)
            {
                for (var i = 0; i < requestsList.Count(); i++)
                {
                    if (requestsList[i].RequestTypeId == 1)
                    {
                        var days = (requestsList[i].EndDate - requestsList[i].StartDate).Days + 1;
                        usedAmount += days;
                    }
                }
                if (usedAmount + requestAmount > totalAmount)
                {
                    toReturn.Id = 3;
                    toReturn.MessageStr = "Not enough entitlements.";
                    return Json(new { data = toReturn });
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
            var success = AddNewRequest(currentUserId, requestTypeId, startDate, endDate);
            if (success)
            {
                toReturn.Id = 1;
                toReturn.MessageStr = "";
                return Json(new { data = toReturn });
            }
            Message errorMessage = new Message
            {
                Id = -1,
                MessageStr = "There was an error with processing your request."
            };
            return Json(new { data = errorMessage });

        }
    }
}

