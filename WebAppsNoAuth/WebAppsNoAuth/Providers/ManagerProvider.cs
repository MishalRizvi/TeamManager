using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using WebAppsNoAuth.Data;
using WebAppsNoAuth.Models;

namespace WebAppsNoAuth.Providers
{
	public class ManagerProvider
	{
        private readonly SqlConnection _connection;
        private readonly WebAppsNoAuthDbContext _webApps;
        private readonly IConfiguration _configuration;


        public ManagerProvider(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
        {
            _webApps = webApps;
            _configuration = configuration;
            // _connection = connection;
            _connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb"));

        }

        public List<Dropdown> GetAllManagers(int userId, int institutionId)
        {
            List<Dropdown> allManagers = new List<Dropdown>();
            try
            {
                _connection.Open();
                var queryString = "SELECT Id, Name FROM [Users] WHERE Manager = @TRUE AND InstitutionId = @INSTITUTIONID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, _connection);
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
                    _connection.Close();
                    return allManagers;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public List<User> GetAllManagerUsers(int managerUserId) //get all the users who the manager manages 
        {
            List<User> allUsers = new List<User>();
            try
            {
                _connection.Open();
                var queryString = "SELECT U.[Id], U.[Name] FROM Users U WHERE U.[Id] = @USERID " +
                                    "UNION SELECT U.[Id], U.[Name] FROM Users U WHERE U.[ManagerUserId] = @USERID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", managerUserId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        User currentUser = new User();
                        currentUser.Id = dbReader.GetInt32(0);
                        currentUser.Name = dbReader.GetString(1);
                        allUsers.Add(currentUser);
                    }
                    _connection.Close();
                    return allUsers;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }
    }
}

