using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebAppsNoAuth.Data;
using WebAppsNoAuth.Models;

namespace WebAppsNoAuth.Providers
{
	public class LocationProvider
	{
        private readonly SqlConnection _connection;
        private readonly WebAppsNoAuthDbContext _webApps;
        private readonly IConfiguration _configuration;


        public LocationProvider(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
        {
            _webApps = webApps;
            _configuration = configuration;
            _connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb"));

        }

        public List<Location> GetAllLocations(int userId, int institutionId)
        {
            List<Location> allLocations = new List<Location>();
            try
            {
                _connection.Open();
                var queryString = "SELECT * FROM [Location] WHERE InstitutionId = @INSTITUTIONID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        Location currentLocation = new Location();
                        currentLocation.LocationId = dbReader.GetInt32(0);
                        currentLocation.LocationValue = dbReader.GetString(1);
                        currentLocation.LocationTitle = dbReader.GetString(2);
                        allLocations.Add(currentLocation);
                    }
                    _connection.Close();
                    return allLocations;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public Location GetLocationById(int locationId)
        {
            Location currentLocation = new Location();
            try
            {
                _connection.Open();
                var queryString = "SELECT LocationId, LocationValue, LocationTitle FROM [Location] WHERE LocationId = @LOCATIONID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@LOCATIONID", locationId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())

                    {
                        currentLocation.LocationId = dbReader.GetInt32(0);
                        currentLocation.LocationValue = dbReader.GetString(1);
                        currentLocation.LocationTitle = dbReader.GetString(2);
                    }
                    _connection.Close();
                    return currentLocation;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public List<Team> GetAllLocationTeams(int locationId, int userId, int institutionId)
        {
            List<Team> allTeams = new List<Team>();
            try
            {
                _connection.Open();
                var queryString = "SELECT * FROM [Team] WHERE LocationId = @LOCATIONID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@LOCATIONID", locationId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        Team currentTeam = new Team();
                        currentTeam.TeamId = dbReader.GetInt32(0);
                        currentTeam.TeamName = dbReader.GetString(1);
                        currentTeam.LocationId = dbReader.GetInt32(2);
                        allTeams.Add(currentTeam);
                    }
                    _connection.Close();
                    return allTeams;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
            
        }

        public bool AddNewLocation(string locationValue, string locationTitle, int userId, int institutionId)
        {
            try
            {
                _connection.Open();
                Debug.WriteLine(locationValue);

                var queryString = "INSERT INTO [Location] VALUES (@LOCATIONVALUE,@LOCATIONTITLE,@INSTITUTIONID)";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@LOCATIONVALUE", locationValue);
                command.Parameters.AddWithValue("@LOCATIONTITLE", locationTitle);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
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

        public bool UpdateLocationMethod(int updateLocationId, string locationValue, string locationTitle, int currentUserId)
        {
            try
            {
                _connection.Open();

                var queryString = "UPDATE [Location] SET LocationValue = @LOCATIONVALUE, LocationTitle = @LOCATIONTITLE WHERE LocationId = @LOCATIONID";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@LOCATIONVALUE", locationValue);
                command.Parameters.AddWithValue("@LOCATIONTITLE", locationTitle);
                command.Parameters.AddWithValue("@LOCATIONID", updateLocationId);
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


        //find all the teams that belong to the location
        //Delete all team users that belong to these teams
        //delete all teams that belong to the location
        //delete the location
        public bool DeleteLocation(int locationId) //will have to update all tables so that instead of deleting you set Active to 0
        {
            try
            {
                var teamId = -1;
                List<Int32> teamIds = new List<Int32>();
                _connection.Open();

                var queryString = "SELECT TeamId FROM [Team] WHERE LocationId = @LOCATIONID;";

                var queryString2 = "DELETE FROM [TeamUser] WHERE TeamId = @TEAMID;";

                var queryString3 = "DELETE FROM [Team] WHERE LocationID = @LOCATIONID;" +
                                   "DELETE FROM [Location] WHERE LocationId = @LOCATIONID";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@LOCATIONID", locationId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        teamId = dbReader.GetInt32(0);
                        teamIds.Add(teamId);
                    }
                    _connection.Close();
                }
                _connection.Close();

                foreach (var team in teamIds)
                {
                    _connection.Open();
                    SqlCommand command2 = new SqlCommand(queryString2, _connection);
                    command2.Parameters.AddWithValue("@TEAMID", team);
                    command2.ExecuteNonQuery();
                    _connection.Close();
                }

                SqlCommand command3 = new SqlCommand(queryString3, _connection);
                command3.Parameters.AddWithValue("@LOCATIONID", locationId);
                _connection.Open();
                command3.ExecuteNonQuery();
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

