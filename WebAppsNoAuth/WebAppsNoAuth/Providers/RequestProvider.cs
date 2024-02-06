using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebAppsNoAuth.Data;
using WebAppsNoAuth.Models;

namespace WebAppsNoAuth.Providers
{
    public class RequestProvider
    {
        private readonly SqlConnection _connection;
        private readonly WebAppsNoAuthDbContext _webApps;
        private readonly IConfiguration _configuration;

        public RequestProvider(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
        {
            _webApps = webApps;
            _configuration = configuration;
            _connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb"));

        }

        public List<RequestType> GetAllRequestTypes()
        {
            List<RequestType> allRequests = new List<RequestType>();
            try
            {
                _connection.Open();
                var queryString = "SELECT * FROM RequestType";
                SqlCommand command = new SqlCommand(queryString, _connection);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        RequestType requestType = new RequestType();
                        requestType.RequestTypeId = dbReader.GetInt32(0);
                        requestType.RequestTypeName = dbReader.GetString(1);
                        allRequests.Add(requestType);
                    }
                    _connection.Close();
                    return allRequests;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public List<Request> GetAllRequests(int userId)
        {
            List<Request> allRequests = new List<Request>();
            try
            {
                _connection.Open();
                var queryString = "SELECT R.[RequestId], R.[RequestTypeId], R.[UserId], R.[StartDate], R.[EndDate], RT.[RequestTypeName], R.[Approved], R.[Description] FROM Request R " +
                                    "JOIN RequestType RT ON R.[RequestTypeId] = RT.[RequestTypeId] WHERE R.[UserId] = @USERID";
                SqlCommand command = new SqlCommand(queryString, _connection);
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
                        currentRequest.Description = dbReader.IsDBNull(7) ? "" : dbReader.GetString(7);
                        allRequests.Add(currentRequest);
                    }
                    _connection.Close();

                    allRequests = allRequests.OrderBy(e => e.StartDate).ToList();
                    return allRequests;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }

        }

        public List<Request> GetAllApprovedRequests(int userId)
        {
            Console.WriteLine("in getallappinprovider: " + userId);
            List<Request> allRequests = new List<Request>();
            try
            {
                _connection.Open();
                var queryString = "SELECT R.[RequestId], R.[RequestTypeId], R.[UserId], R.[StartDate], R.[EndDate], RT.[RequestTypeName], R.[Approved], R.[Description], U.[Name] FROM Request R " +
                                  "JOIN RequestType RT ON R.[RequestTypeId] = RT.[RequestTypeId] " +
                                  "JOIN Users U ON R.[UserId] = U.[Id] WHERE R.[UserId] = @USERID AND R.[Approved] = @TRUE";
                SqlCommand command = new SqlCommand(queryString, _connection);
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
                        currentRequest.Description = dbReader.IsDBNull(7) ? "" : dbReader.GetString(7);
                        currentRequest.UserName = dbReader.GetString(8);
                        allRequests.Add(currentRequest);
                    }
                    _connection.Close();
                    return allRequests;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }

        }
        public List<List<Request>> GetFilteredRequests(IEnumerable<Int32> usersList) //filtered requests that are APPROVED
        {
            List<List<Request>> allRequests = new List<List<Request>>();
            for (var i = 0; i < usersList.Count(); i++)
            {
                List<Request> userRequests = new List<Request>();
                try
                {
                    _connection.Open();
                    var queryString = "SELECT R.[RequestId], R.[RequestTypeId], R.[UserId], R.[StartDate], R.[EndDate], RT.[RequestTypeName], R.[Approved], R.[Description], U.[Name] FROM Request R " +
                                      "JOIN RequestType RT ON R.[RequestTypeId] = RT.[RequestTypeId] " +
                                      "JOIN Users U ON R.[UserId] = U.[Id] WHERE R.[UserId] = @USERID";
                    SqlCommand command = new SqlCommand(queryString, _connection);
                    command.Parameters.AddWithValue("@USERID", usersList.ElementAt(i));
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
                            currentRequest.Description = dbReader.IsDBNull(7) ? "" : dbReader.GetString(7);
                            currentRequest.UserName = dbReader.GetString(8);
                            userRequests.Add(currentRequest);
                        }
                        _connection.Close();

                        allRequests.Add(userRequests); 
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return null;
                }
            }
            return allRequests;

        }


        public List<Request> GetUserRequests(int userId)
        {
            List<Request> allRequests = new List<Request>();
            try
            {
                _connection.Open();
                var queryString = "SELECT R.[RequestId], R.[RequestTypeId], R.[UserId], R.[StartDate], R.[EndDate], RT.[RequestTypeName], R.[Approved], R.[Description] FROM Request R " +
                                  "JOIN RequestType RT ON R.[RequestTypeId] = RT.[RequestTypeId] WHERE R.[UserId] = @USERID";
                SqlCommand command = new SqlCommand(queryString, _connection);
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
                        currentRequest.Description = dbReader.IsDBNull(7) ? "" : dbReader.GetString(7);
                        allRequests.Add(currentRequest);
                    }
                    _connection.Close();
                    return allRequests;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public Request GetRequestById(int requestId)
        {
            List<Request> allRequests = new List<Request>();
            Request currentRequest = new Request();
            try
            {
                _connection.Open();
                var queryString = "SELECT R.[RequestId], R.[RequestTypeId], R.[UserId], R.[StartDate], R.[EndDate], RT.[RequestTypeName], R.[Approved], R.[Description] FROM Request R " +
                                    "JOIN RequestType RT ON R.[RequestTypeId] = RT.[RequestTypeId] WHERE R.[RequestId] = @REQUESTID";
                SqlCommand command = new SqlCommand(queryString, _connection);
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
                        currentRequest.Description = dbReader.IsDBNull(7) ? "" : dbReader.GetString(7);
                    }
                    _connection.Close();
                    return currentRequest;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public List<Request> GetAllManagerRequests(int userId) 
        {
            List<Request> allRequests = new List<Request>();
            try
            {
                _connection.Open();
                var queryString = "SELECT R.[RequestId], R.[RequestTypeId], R.[UserId], R.[StartDate], R.[EndDate], RT.[RequestTypeName], " + 
                                  "R.[Approved], R.[Description], U.[Name] FROM Request R " +
                                  "JOIN RequestType RT ON R.[RequestTypeId] = RT.[RequestTypeId] " +
                                  "JOIN Users U ON R.[UserId] = U.[Id] WHERE U.[ManagerUserId] = @USERID AND R.[Approved] IS NULL";
                SqlCommand command = new SqlCommand(queryString, _connection);
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
                        currentRequest.Description = dbReader.IsDBNull(7) ? "" : dbReader.GetString(7);
                        currentRequest.UserName = dbReader.GetString(8);
                        allRequests.Add(currentRequest);
                    }
                    _connection.Close();
                    return allRequests;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public bool AddNewRequest(int userId, int requestTypeId, DateTime startDate, DateTime endDate, string requestDescription)
        {
            try
            {
                var queryString = "";
                _connection.Open();
                if (requestDescription == "NONE")
                {
                    queryString = "INSERT INTO [Request] VALUES (@REQUESTTYPEID,@USERID,@STARTDATE,@ENDDATE,NULL,NULL)"; //what if manager is doing this on behalf?
                }
                else
                {
                    queryString = "INSERT INTO [Request] VALUES (@REQUESTTYPEID,@USERID,@STARTDATE,@ENDDATE,NULL,@REQUESTDESCRIPTION)"; //what if manager is doing this on behalf?
                }
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@REQUESTTYPEID", requestTypeId);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@STARTDATE", startDate);
                command.Parameters.AddWithValue("@ENDDATE", endDate);
                command.Parameters.AddWithValue("@REQUESTDESCRIPTION", requestDescription);
                command.ExecuteNonQuery();

                _connection.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public bool DeleteRequest(int requestId)
        {
            try
            {
                _connection.Open();

                var queryString = "DELETE FROM [Request] WHERE RequestId = @REQUESTID"; //what if manager is doing this on behalf?
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@REQUESTID", requestId);
                command.ExecuteNonQuery();

                _connection.Close();
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        
    }

        public bool ApproveRequestMethod(int requestId)
        {            
            try
            {
                _connection.Open();

                var queryString = "UPDATE [Request] SET Approved = @TRUE WHERE RequestId = @REQUESTID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@REQUESTID", requestId);
                command.Parameters.AddWithValue("@TRUE", true);
                command.ExecuteNonQuery();

                _connection.Close();
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
            
        }

        public bool RejectRequestMethod(int requestId)
        {
            try
            {
                _connection.Open();

                var queryString = "UPDATE [Request] SET Approved = @FALSE WHERE RequestId = @REQUESTID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@REQUESTID", requestId);
                command.Parameters.AddWithValue("@FALSE", false);
                command.ExecuteNonQuery();

                _connection.Close();

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }  
    }
}

