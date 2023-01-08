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

        public ManagerController(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
        {
            _webApps = webApps;
            _configuration = configuration;
        }

        // GET: /<controller>/
        public IActionResult ApproveRequest()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]);
            Debug.WriteLine("USER ID");
            Debug.WriteLine(userId);
            ViewData["Authenticated"] = "true";
            ViewData["Username"] = HttpContext.User.Claims.ToList()[2].ToString().Split(":")[1];
            ViewData["Admin"] = IsUserAdmin(userId);
            ViewData["Manager"] = IsUserManager(userId);
            Debug.WriteLine(ViewData["Manager"]);
            return View();
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

        //Manager getting another users requests
        public Request GetRequestById(int requestId)
        {
            List<Request> allRequests = new List<Request>();
            Request currentRequest = new Request();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT R.[RequestId], R.[RequestTypeId], R.[UserId], R.[StartDate], R.[EndDate], RT.[RequestTypeName], R.[Approved] FROM Request R " +
                                  "JOIN RequestType RT ON R.[RequestTypeId] = RT.[RequestTypeId] WHERE R.[RequestId] = @REQUESTID";
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@REQUESTID", requestId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
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
                    }
                    connection.Close();
                }
            }
            return currentRequest;
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
            var userName = GetUserName(userId);
            var userEmail = GetUserEmail(userId);
            var managerId = GetManager(userId);
            var managerName = GetUserName(managerId);

            emailTemplate.Body = "Hi " + userName + "," + Environment.NewLine +
                                 "Your request:" + Environment.NewLine +
                                 "Request Type: " + requestTypeName + Environment.NewLine +
                                 "Start Date: " + requestStartDate + Environment.NewLine +
                                 "End Date: " + requestEndDate + Environment.NewLine +
                                 "has been approved by " + managerName + ".";

            emailTemplate.To = userEmail;
            emailTemplate.ToName = userName;
            var success = SendEmail(emailTemplate);
            if (success)
            {
                return true;
            }
            return false;
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
            var userName = GetUserName(userId);
            var userEmail = GetUserEmail(userId);
            var managerId = GetManager(userId);
            var managerName = GetUserName(managerId);

            emailTemplate.Body = "Hi " + userName + "," + Environment.NewLine +
                                 "Your request:" + Environment.NewLine +
                                 "Request Type: " + requestTypeName + Environment.NewLine +
                                 "Start Date: " + requestStartDate + Environment.NewLine +
                                 "End Date: " + requestEndDate + Environment.NewLine +
                                 "has been rejected by " + managerName + ".";
                                
            emailTemplate.To = userEmail;
            emailTemplate.ToName = userName;
            var success = SendEmail(emailTemplate);
            if (success)
            {
                return true;
            }
            return false;
        }

        public bool SendEmail(EmailTemplate emailTemplate)
        {
            MailMessage m = new MailMessage();
            SmtpClient sc = new SmtpClient();
            m.From = new MailAddress(_configuration.GetSection("EmailConfiguration")["From"], "Services App For You");
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

        //THE ABOVE METHODS NEED TO BE MOVED TO THEIR RESPECTIVE CONTROLLER METHODS 

        public ActionResult GetAllManagerRequests()
        {
            int userId = Int32.Parse(HttpContext.User.Claims.ToList()[1].ToString().Split(":")[1]); //this is the managerUserId
            List<Request> allRequests = new List<Request>();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();
                var queryString = "SELECT R.[RequestId], R.[RequestTypeId], R.[UserId], R.[StartDate], R.[EndDate], RT.[RequestTypeName], R.[Approved], U.[Name] FROM Request R " +
                                  "JOIN RequestType RT ON R.[RequestTypeId] = RT.[RequestTypeId] " +
                                  "JOIN Users U ON R.[UserId] = U.[Id] WHERE U.[ManagerUserId] = @USERID AND R.[Approved] IS NULL";
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
                        currentRequest.UserName = dbReader.GetString(7);
                        allRequests.Add(currentRequest);
                    }
                    connection.Close();
                }
            }
            return Json(new { data = allRequests });
        }

        public bool ApproveRequestMethod(int requestId)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "UPDATE [Request] SET Approved = @TRUE WHERE RequestId = @REQUESTID"; 
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@REQUESTID", requestId);
                command.Parameters.AddWithValue("@TRUE", true);
                command.ExecuteNonQuery();

                connection.Close();

                var success = SendApproveRequestEmail(requestId);
                if (success)
                {
                    return true;
                }
            }

            return false;
        }

        public bool RejectRequestMethod(int requestId)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
            {
                connection.Open();

                var queryString = "UPDATE [Request] SET Approved = @FALSE WHERE RequestId = @REQUESTID"; 
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@REQUESTID", requestId);
                command.Parameters.AddWithValue("@FALSE", false);
                command.ExecuteNonQuery();

                connection.Close();

                var success = SendRejectRequestEmail(requestId);
                if (success)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

