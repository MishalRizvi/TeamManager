using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebAppsNoAuth.Data;
using WebAppsNoAuth.Models;

namespace WebAppsNoAuth.Providers
{
	public class TeamProvider
	{
        private readonly SqlConnection _connection;
        private readonly WebAppsNoAuthDbContext _webApps;
        private readonly IConfiguration _configuration;


        public TeamProvider(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
        {
            _webApps = webApps;
            _configuration = configuration;
            _connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb"));

        }

        public Team GetTeamById(int teamId)
        {
            Team currentTeam = new Team();
            try
            { 
                _connection.Open();
                var queryString = "SELECT * FROM [Team] WHERE TeamId = @TEAMID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@TEAMID", teamId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())

                    {
                        currentTeam.TeamId = dbReader.GetInt32(0);
                        currentTeam.TeamName = dbReader.GetString(1);
                        currentTeam.LocationId = dbReader.GetInt32(2);
                    }
                    _connection.Close();
                    return currentTeam;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public bool AddNewTeam(string teamName, int locationId, int userId, int institutionId)
        {
            try
            {
                _connection.Open();

                var queryString = "INSERT INTO [Team] VALUES (@TEAMNAME,@LOCATIONID)";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@TEAMNAME", teamName);
                command.Parameters.AddWithValue("@LOCATIONID", locationId);
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

        //here contact number is not added, this option is available under profile icon 
        public bool AddUserToTeam(int userId, int teamId)
        {
            try 
            {
                _connection.Open();

                var queryString = "INSERT INTO [TeamUser] VALUES (@USERID,@TEAMID)";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@TEAMID", teamId);
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

        public bool RemoveUserFromTeam(int userId, int teamId)
        {
            try
            {
                _connection.Open();

                var queryString = "DELETE FROM [TeamUser] WHERE UserId = @USERID AND TeamId = @TEAMID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@TEAMID", teamId);
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

        public bool UpdateTeamMethod(int updateTeamId, string teamName, int currentUserId)
        {
            try
            {
                _connection.Open();

                var queryString = "UPDATE [Team] SET TeamName = @TEAMNAME WHERE TeamId = @TEAMID";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@TEAMNAME", teamName);
                command.Parameters.AddWithValue("@TEAMID", updateTeamId);
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

        public bool DeleteTeam(int teamId) //will have to update all tables so that instead of deleting you set Active to 0
        {
            try
            {
                _connection.Open();

                var queryString = "DELETE FROM [Team] WHERE TeamId = @TEAMID";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@TEAMID", teamId);
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

