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
                        currentLocation.LocationName = dbReader.GetString(1);
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
                var queryString = "SELECT * FROM [Location] WHERE LocationId = @LOCATIONID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@LOCATIONID", locationId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())

                    {
                        currentLocation.LocationId = dbReader.GetInt32(0);
                        currentLocation.LocationName = dbReader.GetString(1);
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

        public bool AddNewLocation(string locationName, string locationTitle, int userId, int institutionId)
        {
            try
            {
                _connection.Open();

                var queryString = "INSERT INTO [Location] VALUES (@LOCATIONNAME,@LOCATIONTITLE,@INSTITUTIONID)";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@LOCATIONNAME", locationName);
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

        public bool UpdateLocationMethod(int updateLocationId, string locationName, string locationTitle, int currentUserId)
        {
            try
            {
                _connection.Open();

                var queryString = "UPDATE [Location] SET LocationName = @LOCATIONNAME, LocationTitle = @LOCATIONTITLE WHERE LocationId = @LOCATIONID";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@LOCATIONNAME", locationName);
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

        public bool DeleteLocation(int locationId) //will have to update all tables so that instead of deleting you set Active to 0
        {
            try
            {
                _connection.Open();

                var queryString = "DELETE FROM [Location] WHERE LocationId = @LOCATIONID";

                SqlCommand command = new SqlCommand(queryString, _connection);
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
    }
}

