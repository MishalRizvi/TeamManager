using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities.Zlib;
using WebAppsNoAuth.Data;
using WebAppsNoAuth.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WebAppsNoAuth.Providers
{
	public class UserProvider
	{
		private readonly SqlConnection _connection;
        private readonly WebAppsNoAuthDbContext _webApps;
        private readonly IConfiguration _configuration;



        public UserProvider(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
		{
            _webApps = webApps;
            _configuration = configuration;
            _connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb"));

        }

        public bool IsUserAdmin(int userId)
        {
            bool isAdmin = false;
            try
            {
                _connection.Open();

                var queryString = "SELECT Admin FROM [Users] WHERE Id = @ID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@ID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        if (dbReader.IsDBNull(0))
                        {
                            break;
                        }
                        isAdmin = dbReader.GetBoolean(0);
                    }
                }
                _connection.Close();
                return isAdmin;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return isAdmin;
            }
        }

        public bool IsUserManager(int userId)
        {
            bool isManager = false;
            try
            {
                _connection.Open();
                var queryString = "SELECT Manager FROM [Users] WHERE Id = @ID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@ID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        if (dbReader.IsDBNull(0))
                        {
                            break;
                        }
                        isManager = dbReader.GetBoolean(0);
                    }
                }
                _connection.Close();
                return isManager;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }  
        }

        public int GetInstitutionId(int userId)
        {
            int institutionId = -1;
            try
            {
                _connection.Open();
                var queryString = "SELECT InstitutionID FROM [Users] WHERE Id = @USERID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        if (dbReader.IsDBNull(0))
                        {
                            break;
                        }
                        institutionId = dbReader.GetInt32(0);
                    }
                }
                _connection.Close();
                Debug.WriteLine(institutionId);
                return institutionId;

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return institutionId;
            }
        }

        public string GetUserName(int userId)
        {
            var userName = "";
            try
            {
                _connection.Open();
                var queryString = "SELECT Name FROM [Users] WHERE Id = @USERID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        if (dbReader.IsDBNull(0))
                        {
                            break;
                        }
                        userName = dbReader.GetString(0);
                    }
                }
                _connection.Close();
                return userName;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return userName;
            }
        }

        public string GetUserEmail(int userId)
        {
            var userEmail = "";
            try
            {
                _connection.Open();
                var queryString = "SELECT Email FROM [Users] WHERE Id = @USERID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        if (dbReader.IsDBNull(0))
                        {
                            break;
                        }
                        userEmail = dbReader.GetString(0);
                    }
                }
                _connection.Close();
                return userEmail;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return userEmail;
            }
        }

        public int GetManager(int userId)
        {
            var managerUserId = -1;
            try
            {
                _connection.Open();
                var queryString = "SELECT ManagerUserId FROM [Users] WHERE Id = @USERID";
                SqlCommand command = new SqlCommand(queryString, _connection);
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
                    _connection.Close();
                    return managerUserId;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return managerUserId;
            }
        }

        public bool AddUser2(User userIn, int institutionID) //This is for when admin creates an account AND an instiution 
        {
            var addedUserId = -1;
            try
            {
                _connection.Open();

                var queryString = "INSERT INTO [Users] VALUES (@NAME, @EMAIL,@PASSWORD,NULL,NULL,@ADMIN,@FALSE,@INSTITUTIONID); " +
                                        "SELECT CAST(SCOPE_IDENTITY() AS INT)";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@NAME", userIn.Name);
                command.Parameters.AddWithValue("@EMAIL", userIn.Email);
                var passwordHashed = HashPassword(userIn.Password);
                command.Parameters.AddWithValue("@PASSWORD", passwordHashed);
                command.Parameters.AddWithValue("@ADMIN", true);
                command.Parameters.AddWithValue("@FALSE", false);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionID);

                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        addedUserId = dbReader.GetInt32(0);
                    }
                }
                _connection.Close();

                var success = SetUserEntitlements(addedUserId);
                if (success)
                {
                    var success2 = SetUserInitialStatus(addedUserId, 2);
                    if (success2)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public string HashPassword(string password)
        {
            byte[] salt;
            var saltSize = 16;
            var hashSize = 10;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[saltSize]);

            var iterations = 1000;
            //create hash
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            var hash = pbkdf2.GetBytes(hashSize);

            //combine salt and hash
            var hashBytes = new byte[saltSize + hashSize];
            Array.Copy(salt, 0, hashBytes, 0, saltSize);
            Array.Copy(hash, 0, hashBytes, saltSize, hashSize);

            //convert to base64
            var base64Hash = Convert.ToBase64String(hashBytes);

            //format hash with extra information
            return string.Format("$MYHASH$V1${0}${1}", iterations, base64Hash);
        }

 

        //public bool SetAdminEntitlements(int userId)
        //{
        //    try
        //    {
        //        _connection.Open();

        //        var queryString = "INSERT INTO [UserEntitlements] VALUES (@USERID, 0, @CURRENTYEAR)";
        //        SqlCommand command = new SqlCommand(queryString, _connection);
        //        command.Parameters.AddWithValue("@USERID", userId);
        //        command.Parameters.AddWithValue("@CURRENTYEAR", DateTime.Today.Year); //change so you can recieve the proper one from updateuser.cshtml
        //        command.ExecuteNonQuery();

        //        _connection.Close();
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(e);
        //        return false;
        //    }
        //}

        public List<User> GetAllUsers(int userId)
        {
            var institutionId = GetInstitutionId(userId);
            List<User> allUsers = new List<User>();
            try
            {
                _connection.Open();
                var queryString = "SELECT U.[Id], U.[Name], U.[Email], U.[Password], U.[ContactNumber], U.[ManagerUserId], U2.[Name], " +
                                  "U.[Admin], U.[Manager] FROM [Users] U " +
                                  "LEFT JOIN [Users] U2 ON U.[ManagerUserId] = U2.[Id] " +
                                  "WHERE U.[InstitutionId] = @INSTITUTIONID"; //Left join to allow retrieval of users with null ManagerUserId 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        User currentUser = new User();
                        currentUser.Id = dbReader.GetInt32(0);
                        currentUser.Name = dbReader.GetString(1);
                        currentUser.Email = dbReader.GetString(2);
                        currentUser.Password = dbReader.GetString(3);
                        currentUser.ContactNumber = dbReader.IsDBNull(4) ? "" : dbReader.GetString(4);
                        currentUser.ManagerUserId = dbReader.IsDBNull(5) ? -1 : dbReader.GetInt32(5); //if manager id is -1, then return as empty string
                        currentUser.ManagerUserName = dbReader.IsDBNull(6) ? "" : dbReader.GetString(6);
                        currentUser.Admin = dbReader.IsDBNull(7) ? false : dbReader.GetBoolean(7);
                        currentUser.Manager = dbReader.IsDBNull(8) ? false : dbReader.GetBoolean(8);
                        if (currentUser.Id == userId)
                        {
                            allUsers.Insert(0, currentUser);
                        }
                        else
                        {
                            allUsers.Add(currentUser);
                        }
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

        public List<User> GetAllUsersInLocation(int userId, int locationId) //CAN GET RID OF USERID HERE AND INSTITUTIONID
        {
            var institutionId = GetInstitutionId(userId);
            List<User> allUsers = new List<User>();
            try
            {
                _connection.Open();
                var queryString = "SELECT DISTINCT U.[Id], U.[Name], U.[Email], U.[Password], TU.[ContactNumber], U.[ManagerUserId], U2.[Name], U.[Admin], U.[Manager] FROM [Users] U " +
                                  "LEFT JOIN [Users] U2 ON U.[ManagerUserId] = U2.[Id] " +
                                  "LEFT JOIN [TeamUser] TU ON U.[Id] = TU.[UserId] " +
                                  "JOIN [Team] T ON TU.[TeamId] = T.[TeamId] " + 
                                  "WHERE U.[InstitutionId] = @INSTITUTIONID AND T.[LocationId] = @LOCATIONID"; //Left join to allow retrieval of users with null ManagerUserId 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
                command.Parameters.AddWithValue("@LOCATIONID", locationId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        User currentUser = new User();
                        currentUser.Id = dbReader.GetInt32(0);
                        currentUser.Name = dbReader.GetString(1);
                        currentUser.Email = dbReader.GetString(2);
                        currentUser.Password = dbReader.GetString(3);
                        currentUser.ContactNumber = dbReader.IsDBNull(4) ? "" : dbReader.GetString(4);
                        currentUser.ManagerUserId = dbReader.IsDBNull(5) ? -1 : dbReader.GetInt32(5);
                        currentUser.ManagerUserName = dbReader.IsDBNull(6) ? "" : dbReader.GetString(6);
                        currentUser.Admin = dbReader.IsDBNull(7) ? false : dbReader.GetBoolean(7);
                        currentUser.Manager = dbReader.IsDBNull(8) ? false : dbReader.GetBoolean(8);
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

        public IEnumerable<User> GetAllUsersAsObject(int userId)
        {
            var institutionId = GetInstitutionId(userId);
            User user = new User();

            List<User> allUsers = new List<User>();
            IEnumerable<User> toReturn = new List<User>();
            try
            {
                _connection.Open();
                var queryString = "SELECT U.[Id], U.[Name] FROM Users U WHERE Id = @USERID " +
                                    "UNION SELECT U.[Id], U.[Name] FROM Users U WHERE InstitutionId = @INSTITUTIONID"; //Left join to allow retrieval of users with null ManagerUserId 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        User currentUser = new User();
                        currentUser.Id = dbReader.GetInt32(0);
                        currentUser.Name = dbReader.GetString(1);
                        if (currentUser.Id == userId)
                        {
                            user.Id = currentUser.Id;
                            user.Name = currentUser.Name;
                            continue;
                        }
                        else
                        {
                            allUsers.Add(currentUser);
                        }
                    }
                    _connection.Close();
                    toReturn = allUsers.Prepend(user);
                }
                return toReturn;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }


        //dont get contact numbers here as not needed
        public List<User> GetAllUsersInTeam(int teamId)
        {
            List<User> allUsers = new List<User>();
            if (teamId == -1)
            {
                User dummy = new User();
                dummy.Id = -1;
                allUsers.Add(dummy);
                return allUsers;
            }
            try
            {
                _connection.Open();
                var queryString = "SELECT U.[Id], U.[Name] FROM [Users] U JOIN [TeamUser] TU ON TU.[UserId] = U.[Id] WHERE TU.[TeamId] = @TEAMID"; //Left join to allow retrieval of users with null ManagerUserId 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@TEAMID", teamId);
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

        public List<User> GetAllUsersNotInTeam(int teamId, int userId, int institutionId)
        {
            List<int> usersInTeamId = GetAllUsersInTeam(teamId).Select(o => o.Id).ToList(); //take into consideration null lists
            List<User> allUsers = new List<User>();
            try
            {
                _connection.Open();
                var queryString = "SELECT U.[Id], U.[Name] FROM [Users] U WHERE InstitutionId = @INSTITUTIONID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        User currentUser = new User();
                        currentUser.Id = dbReader.GetInt32(0);
                        if (usersInTeamId.Contains(currentUser.Id))
                        {
                            continue;
                        }
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

        public User GetUserById(int userId)
        {
          User currentUser = new User();
          try
            {
                _connection.Open();
                var queryString = "SELECT * FROM [Users] WHERE Id = @USERID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())

                    {
                        currentUser.Id = dbReader.GetInt32(0);
                        currentUser.Name = dbReader.GetString(1);
                        currentUser.Email = dbReader.GetString(2);
                        currentUser.Password = dbReader.GetString(3);
                        currentUser.ContactNumber = dbReader.IsDBNull(4) ? "" : dbReader.GetString(4);
                        currentUser.ManagerUserId = dbReader.IsDBNull(5) ? -1 : dbReader.GetInt32(5);
                        currentUser.Admin = dbReader.IsDBNull(6) ? false : dbReader.GetBoolean(6);
                        currentUser.Manager = dbReader.IsDBNull(7) ? false : dbReader.GetBoolean(7);
                        currentUser.InstitutionId = dbReader.GetInt32(8);
                    }
                    _connection.Close();
                    return currentUser;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public UserEntitlements GetUserEntitlementsById(int userId)
        {
            UserEntitlements currentUserEntitlements = new UserEntitlements();
            try
            {
                _connection.Open();
                var queryString = "SELECT UE.[UserId], UE.[Amount], UE.[Year], U.[Name] FROM [UserEntitlements] UE " +
                                    "JOIN [Users] U ON UE.[UserId] = U.[Id] WHERE UE.[UserId] = @USERID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())

                    {
                        currentUserEntitlements.UserId = dbReader.GetInt32(0);
                        currentUserEntitlements.Amount = dbReader.GetInt32(1);
                        currentUserEntitlements.Year = dbReader.GetInt32(2);
                        currentUserEntitlements.Name = dbReader.GetString(3);

                    }
                    _connection.Close();
                    return currentUserEntitlements;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public int GetUserEntitlements(int userId) //might remove this function
        {
            var amount = -1;
           try { 
                _connection.Open();
                var queryString = "SELECT Amount FROM [UserEntitlements] WHERE UserId = @USERID AND Year = @CURRENTYEAR";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@CURRENTYEAR", DateTime.Today.Year);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        amount = dbReader.GetInt32(0);
                    }
                    _connection.Close();
                    return amount;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return amount;
            }
        }

        public List<UserEntitlements> GetAllEntitlements(int userId, int institutionId)
        {
            List<UserEntitlements> allEntitlements = new List<UserEntitlements>();

            try 
            {
                _connection.Open();
                var queryString = "SELECT UE.[UserId], U.[Name], UE.[Amount] FROM [UserEntitlements] UE " +
                                  "JOIN [Users] U ON UE.[UserId] = U.[Id] WHERE U.[InstitutionId] = @INSTITUTIONID"; // based on year or location dropdown filter on userentitlements page?
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        UserEntitlements currentUserEntitlements = new UserEntitlements();
                        currentUserEntitlements.UserId = dbReader.GetInt32(0);
                        currentUserEntitlements.Name = dbReader.GetString(1);
                        currentUserEntitlements.Amount = dbReader.GetInt32(2);
                        allEntitlements.Add(currentUserEntitlements);
                    }
                    _connection.Close();
                    return allEntitlements;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public bool AddNewUser(string name, string email, string password, string contactNumber, int managerUserId, string isAdmin, string isManager, int userId, int institutionId)
        {
            int addedUserId = -1;

            try
            {
                _connection.Open();

                var queryString = "INSERT INTO [Users] VALUES (@NAME, @EMAIL,@PASSWORD,@CONTACTNUMBER,@MANAGERUSERID,@ADMIN,@MANAGER,@INSTITUTIONID); " +
                                   "SELECT CAST(SCOPE_IDENTITY() AS INT)";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@NAME", name);
                command.Parameters.AddWithValue("@EMAIL", email);
                command.Parameters.AddWithValue("@PASSWORD", password);
                command.Parameters.AddWithValue("@CONTACTNUMBER", contactNumber);
                command.Parameters.AddWithValue("@MANAGERUSERID", managerUserId);
                command.Parameters.AddWithValue("@ADMIN", Convert.ToBoolean(isAdmin)); //change so this val can be set
                command.Parameters.AddWithValue("@MANAGER", Convert.ToBoolean(isManager));
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId); //change so you can recieve the proper one from updateuser.cshtml

                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        addedUserId = dbReader.GetInt32(0);
                    }
                }
                _connection.Close();
                var success = SetUserEntitlements(addedUserId);
                if (success)
                {
                    var success2 = SetUserInitialStatus(addedUserId, 2);
                    if (success2)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
            
        }

        public bool AddNewMeeting(string title, string description, string priority, DateTime meetingDate, string startTime, string endTime, IEnumerable<Int32> attendeesList)
        {
            var addedMeetingId = -1;
            var hostUserId = attendeesList.First(); //from controller method, the first of the list is always the host 
            try
            {
                _connection.Open();

                var queryString = "INSERT INTO [Meeting] VALUES (@TITLE, @HOSTUSERID, @DESCRIPTION, @PRIORITY, @DATE, @STARTTIME, @ENDTIME); " +
                                  "SELECT CAST(SCOPE_IDENTITY() AS INT);";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@TITLE", title);
                command.Parameters.AddWithValue("@HOSTUSERID", hostUserId);
                command.Parameters.AddWithValue("@DESCRIPTION", description);
                command.Parameters.AddWithValue("@PRIORITY", priority);
                command.Parameters.AddWithValue("@DATE", meetingDate);
                command.Parameters.AddWithValue("@STARTTIME", startTime);
                command.Parameters.AddWithValue("@ENDTIME", endTime);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        addedMeetingId = dbReader.GetInt32(0);
                    }
                } 
                _connection.Close();

                for (var i = 0; i < attendeesList.Count(); i++)
                {
                    if (i == 0)
                    {
                        AddNewMeetingAttendee(addedMeetingId, attendeesList.ElementAt(i), 1); //make it active for the host of the meeting 
                    }
                    else
                    {
                        AddNewMeetingAttendee(addedMeetingId, attendeesList.ElementAt(i));
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }

        }

        public void AddNewMeetingAttendee(int meetingId, int userId, int active = 0) //if the meeting is not active, on meeting page will come up with 'Accept' option 
        {
            try
            {
                _connection.Open();

                var queryString = "INSERT INTO [MeetingAttendee] VALUES (@MEETINGID, @USERID, @ACTIVE); "; //set active as 0 by default 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@MEETINGID", meetingId);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@ACTIVE", active);
                command.ExecuteNonQuery();
                _connection.Close();
                return;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return;
            }

        }

        public bool SetUserPassword(int userId, string password) 
        {
            var passwordHashed = HashPassword(password);
            try
            {
                _connection.Open();

                var queryString = "UPDATE [Users] SET Password = @PASSWORD WHERE Id = @USERID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@PASSWORD", passwordHashed); 
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

        public bool SetUserEntitlements(int userId) //set default as 0
        {
            try
            {
                _connection.Open();

                var queryString = "INSERT INTO [UserEntitlements] VALUES (@USERID, 0, @CURRENTYEAR)";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@CURRENTYEAR", DateTime.Today.Year); //change so you can recieve the proper one from updateuser.cshtml
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

        public bool SetUserInitialStatus(int userId, int statusTypeId) //set default as unavailable
        {
            try
            {
                _connection.Open();

                var queryString = "INSERT INTO [Status] VALUES (@USERID, @STATUSTYPEID, NULL, @WFH, NULL)";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@STATUSTYPEID", statusTypeId);
                command.Parameters.AddWithValue("@WFH", false);
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

        public bool SetUserStatus(int userId, int statusTypeId, int locationId, bool isWFH, string wfhContact) //set default as unavailable
        {
            var queryString = "";
            try
            {
                _connection.Open();
                if (locationId != -1)
                {
                    if (isWFH)
                    {
                        queryString = "UPDATE [Status] SET StatusTypeId = @STATUSTYPEID, CurrentLocationId = @LOCATIONID, WFH = @ISWFH, WFHContact = @WFHCONTACT WHERE UserId = @USERID";
                    }
                    else
                    {
                        queryString = "UPDATE [Status] SET StatusTypeId = @STATUSTYPEID, CurrentLocationId = @LOCATIONID, WFH = @ISWFH WHERE UserId = @USERID";
                    }
                }
                else
                {
                    if (isWFH)
                    {
                        queryString = "UPDATE [Status] SET StatusTypeId = @STATUSTYPEID, CurrentLocationId = NULL, WFH = @ISWFH, WFHContact = @WFHCONTACT WHERE UserId = @USERID";
                    }
                    else
                    {
                        queryString = "UPDATE [Status] SET StatusTypeId = @STATUSTYPEID, CurrentLocationId = NULL, WFH = @ISWFH WHERE UserId = @USERID";
                    }
                }
                
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@STATUSTYPEID", statusTypeId);
                command.Parameters.AddWithValue("@LOCATIONID", locationId);
                command.Parameters.AddWithValue("@ISWFH", isWFH);
                command.Parameters.AddWithValue("@WFHCONTACT", wfhContact);
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

        public Status GetUserStatus(int userId) 
        {
            Debug.WriteLine(userId);
            Status currentUserStatus = new Status();
            try
            {
                _connection.Open();

                var queryString = "SELECT S.[UserId], U.[Name], S.[StatusTypeId], ST.[StatusTypeName], S.[CurrentLocationId], L.[LocationValue], S.[WFH], S.[WFHContact] FROM Status S " +
                                  "JOIN StatusType ST ON S.[StatusTypeId] = ST.[StatusTypeId] " +
                                  "JOIN Users U ON U.[Id] = S.[UserId] " +
                                  "LEFT JOIN Location L ON L.[LocationId] = S.[CurrentLocationId] " + //TO ENSURE that when location is empty results are still retrieved
                                  "WHERE S.[UserId] = @USERID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.ExecuteNonQuery();

                using (SqlDataReader dbReader = command.ExecuteReader())
                {

                    while (dbReader.Read())
                    {
                        currentUserStatus.UserId = dbReader.GetInt32(0);
                        currentUserStatus.UserName = dbReader.GetString(1);
                        currentUserStatus.StatusTypeId = dbReader.GetInt32(2);
                        currentUserStatus.StatusTypeName = dbReader.GetString(3);                        
                        currentUserStatus.LocationId = dbReader.IsDBNull(4) ? -1 : dbReader.GetInt32(4);
                        currentUserStatus.LocationValue = dbReader.IsDBNull(4) ? "Unknown" : dbReader.GetString(5);
                        currentUserStatus.WFH = dbReader.IsDBNull(6) ? false : dbReader.GetBoolean(6);
                        currentUserStatus.WFHContact = dbReader.IsDBNull(7) ? "" : dbReader.GetString(7);
                    }
                    _connection.Close();

                    if (currentUserStatus.UserId == 0) {
                        SetUserInitialStatus(userId, 1);
                    }
                    return currentUserStatus;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public List<Status> GetAllUsersStatus(int userId)
        {
            var institutionId = GetInstitutionId(userId);
            List<Status> allUsersStatus = new List<Status>();
            try
            {
                _connection.Open();

                var queryString = "SELECT S.[UserId], U.[Name], S.[StatusTypeId], ST.[StatusTypeName], S.[CurrentLocationId], S.[WFH], S.[WFHContact] FROM Status S " +
                                  "JOIN StatusType ST ON S.[StatusTypeId] = ST.[StatusTypeId] " +
                                  "JOIN Users U ON U.[Id] = S.[UserId] " +
                                  "WHERE U.[InstitutionId] = @INSTITUTIONID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
                command.ExecuteNonQuery();

                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        Status currentUserStatus = new Status();
                        currentUserStatus.UserId = dbReader.GetInt32(0);
                        currentUserStatus.UserName = dbReader.GetString(1);
                        currentUserStatus.StatusTypeId = dbReader.GetInt32(2);
                        currentUserStatus.StatusTypeName = dbReader.GetString(3);
                        currentUserStatus.LocationId = dbReader.IsDBNull(4) ? -1 : dbReader.GetInt32(4);
                        currentUserStatus.WFH = dbReader.IsDBNull(5) ? false : dbReader.GetBoolean(5);
                        currentUserStatus.WFHContact = dbReader.IsDBNull(6) ? "" : dbReader.GetString(6);
                        allUsersStatus.Add(currentUserStatus);
                    }
                    _connection.Close();

                    allUsersStatus = allUsersStatus.OrderBy(e => e.UserName).ToList();
                    return allUsersStatus;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }
        //CHECK THIS ONE
        public string UpdateUserMethod(int userId, string name, string email, string contactNumber, int managerUserId, string isAdmin, string isManager, int currentUserId, bool wasManager) 
        {
            try
            {
                _connection.Open();

                var queryString = "UPDATE [Users] SET Name = @NAME, Email = @EMAIL, ContactNumber = @CONTACTNUMBER, " +
                                   "ManagerUserId = @MANAGERUSERID, Admin = @ADMIN, Manager = @MANAGER WHERE Id = @USERID";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@NAME", name);
                command.Parameters.AddWithValue("@EMAIL", email);
                command.Parameters.AddWithValue("@CONTACTNUMBER", contactNumber);
                command.Parameters.AddWithValue("@MANAGERUSERID", managerUserId);
                command.Parameters.AddWithValue("@ADMIN", Convert.ToBoolean(isAdmin)); //change so this val can be set
                command.Parameters.AddWithValue("@MANAGER", Convert.ToBoolean(isManager));
                command.ExecuteNonQuery();

                _connection.Close();
            }
            catch (Exception e)
            {
                return "";
            }
            if (userId == currentUserId) //If the Admin has updated their own details, need to create a new ClaimsIdentity, hence redirect to login page
            {
                return "false";
            }
            if (wasManager != Convert.ToBoolean(isManager))
            {
                return "changedManager";
            }
            return "true";
        }

        public bool UpdateEntitlementsMethod(int userId, int year, int amount)
        {
            try
            {
                _connection.Open();

                var queryString = "UPDATE [UserEntitlements] SET Amount = @AMOUNT WHERE UserId = @USERID AND Year = @YEAR";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@YEAR", year);
                command.Parameters.AddWithValue("@AMOUNT", amount);
                command.ExecuteNonQuery();

                _connection.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool DeleteUser(int userId) //will have to update all tables so that instead of deleting you set Active to 0
        {
            try
            {
                _connection.Open();

                var queryString = "DELETE FROM [MeetingAttendee] WHERE UserId = @USERID; " +
                                  "DELETE FROM [ProjectPerson] WHERE UserId = @USERID; " +
                                  "DELETE FROM [UserEntitlements] WHERE UserId = @USERID;" +
                                  "DELETE FROM [Status] WHERE UserId = @USERID;" +
                                  "DELETE FROM [Users] WHERE Id = @USERID;";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
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

        public bool AcceptMeeting(int userId, int meetingId) //will have to update all tables so that instead of deleting you set Active to 0
        {
            try
            {
                _connection.Open();

                var queryString = "UPDATE [MeetingAttendee] SET Active = @TRUE WHERE UserId = @USERID AND MeetingId = @MEETINGID;";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@MEETINGID", meetingId);
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

        public bool DeleteMeeting(int userId, int meetingId) //will have to update all tables so that instead of deleting you set Active to 0
        {
            try
            {
                _connection.Open();

                var queryString = "DELETE FROM [MeetingAttendee] WHERE UserId = @USERID AND MeetingId = @MEETINGID;";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@MEETINGID", meetingId);
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

        public List<StatusType> GetAllStatusTypes()
        {
            List<StatusType> allStatuses = new List<StatusType>();
            try
            {
                _connection.Open();
                var queryString = "SELECT * FROM StatusType";
                SqlCommand command = new SqlCommand(queryString, _connection);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        StatusType statusType = new StatusType();
                        statusType.StatusTypeId = dbReader.GetInt32(0);
                        statusType.StatusTypeName = dbReader.GetString(1);
                        allStatuses.Add(statusType);
                    }
                    _connection.Close();
                    return allStatuses;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public List<Location> GetAllUserLocations(int userId)
        {
            List<Location> userLocations = new List<Location>();
            try
            {
                _connection.Open();
                var queryString = "SELECT DISTINCT L.[LocationId], L.[LocationValue], L.[LocationTitle] FROM [Location] L " +
                                  "JOIN Team T ON T.[LocationId] = L.[LocationId] " +
                                  "JOIN TeamUser TU ON T.[TeamId] = TU.[TeamId] " +
                                  "WHERE TU.[UserId] = @USERID"; 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())

                    {
                        Location currentLocation = new Location();
                        currentLocation.LocationId = dbReader.GetInt32(0);
                        currentLocation.LocationValue = dbReader.GetString(1);
                        currentLocation.LocationTitle = dbReader.GetString(2);
                        userLocations.Add(currentLocation);
                    }
                    _connection.Close();
                    return userLocations;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public List<Meeting> GetAllMeetings(int userId)
        {
            List<Meeting> userMeetings = new List<Meeting>();
            try
            {
                _connection.Open();
                var queryString = "SELECT M.[MeetingId], M.[Title], M.[Description], M.[Priority], U.[Name], M.[Date], " +
                                  "M.[StartTime], M.[EndTime], MA.[Active] FROM [Meeting] M " +
                                  "JOIN [MeetingAttendee] MA ON M.[MeetingId] = MA.[MeetingId] " +
                                  "JOIN [Users] U ON M.[HostUserId] = U.[Id] " +
                                  "WHERE MA.[UserId] = @USERID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())

                    {
                        Meeting currentMeeting = new Meeting();
                        currentMeeting.MeetingId = dbReader.GetInt32(0);
                        currentMeeting.Title = dbReader.GetString(1);
                        currentMeeting.Description = dbReader.GetString(2);
                        currentMeeting.Priority = dbReader.GetString(3);
                        currentMeeting.HostUserName = dbReader.GetString(4);
                        currentMeeting.MeetingDate = dbReader.GetDateTime(5).ToString("dd/MM/yyyy");
                        currentMeeting.StartTime = dbReader.GetString(6);
                        currentMeeting.EndTime = dbReader.GetString(7);
                        currentMeeting.Active = dbReader.GetBoolean(8);                        
                        userMeetings.Add(currentMeeting);
                    }
                    _connection.Close();

                    for (var i=0; i<userMeetings.Count(); i++)
                    {
                        userMeetings.ElementAt(i).Attendees = GetMeetingAttendees(userMeetings.ElementAt(i).MeetingId);
                    }

                    userMeetings = userMeetings.OrderBy(e => e.MeetingDate).ThenBy(e => e.StartTime).ToList();

                    return userMeetings;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public Meeting GetMeetingById(int meetingId) //slightly different to GetAllMeetings as we want the host user id so we can email the host from SendAcceptMeetingToHost
        {
            Meeting currentMeeting = new Meeting();
            try
            {
                _connection.Open();

                var queryString = "SELECT M.[MeetingId], M.[Title], M.[Description], M.[Priority], M.[HostUserId], U.[Name], M.[Date], M.[StartTime], M.[EndTime] FROM [Meeting] M " +
                                  "JOIN [Users] U ON M.[HostUserId] = U.[Id] " +
                                  "WHERE M.[MeetingId] = @MEETINGID";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@MEETINGID", meetingId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())

                    {
                        currentMeeting.MeetingId = dbReader.GetInt32(0);
                        currentMeeting.Title = dbReader.GetString(1);
                        currentMeeting.Description = dbReader.GetString(2);
                        currentMeeting.Priority = dbReader.GetString(3);
                        currentMeeting.HostUserId = dbReader.GetInt32(4);
                        currentMeeting.HostUserName = dbReader.GetString(5);
                        currentMeeting.MeetingDate = dbReader.GetDateTime(6).ToString("dd/MM/yyyy");
                        currentMeeting.StartTime = dbReader.GetString(7);
                        currentMeeting.EndTime = dbReader.GetString(8);
                    }
                    _connection.Close();

                    currentMeeting.Attendees = GetMeetingAttendees(meetingId);

                    return currentMeeting;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public string GetMeetingAttendees(int meetingId) //this method gets all attendees WHO HAVE NOT REJECTED THE MEETING, if they reject the meeting their entry in MA table
                                                            //will be deleted hence won't be retrieved in this method
        {
            List<String> attendeeNames = new List<String>();
            var hostNameTwo = "";
            string toReturn = "";
            try
            {
                _connection.Open();
                var queryString = "SELECT U.[Name], MA.[Active] FROM Users U " +
                                  "JOIN MeetingAttendee MA ON U.[Id] = MA.[UserId] " +
                                  "WHERE MA.[MeetingId] = @MEETINGID";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@MEETINGID", meetingId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        var attendeeName = dbReader.GetString(0);
                        var active = dbReader.GetBoolean(1);
                        //if (attendeeName.Equals(hostName)) do we need host name first 
                        //{
                        //    hostNameTwo = attendeeName;
                        //    continue;
                        //}
                        if (!active)
                        {
                            attendeeNames.Add(attendeeName + " Tentative"); //if they have not accepted yet, they have Tentatively accepted 
                        }
                        else
                        {
                            attendeeNames.Add(attendeeName);
                        }
                    }
                    _connection.Close();
                    attendeeNames.Prepend(hostNameTwo); //add host name first

                    for (var i=0; i<attendeeNames.Count; i++)
                    {
                        toReturn += attendeeNames.ElementAt(i);
                        if (i == attendeeNames.Count-1)
                        {
                            break;
                        }
                        toReturn += ",";
                    }
                    return toReturn;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public List<Int32> GetMeetingAttendeesIdList(int meetingId) //this method gets all attendees WHO HAVE NOT REJECTED THE MEETING, if they reject the meeting their entry in MA table
                                                         //will be deleted hence won't be retrieved in this method
        {
            var hostUserIdTwo = -1;

            List<Int32> attendeeIds = new List<Int32>();
            try
            {
                _connection.Open();
                var queryString = "SELECT U.[Id] FROM Users U " +
                                  "JOIN MeetingAttendee MA ON U.[Id] = MA.[UserId] " +
                                  "WHERE MA.[MeetingId] = @MEETINGID";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@MEETINGID", meetingId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        var attendeeId = dbReader.GetInt32(0);
                        attendeeIds.Add(attendeeId);
                    }
                    _connection.Close();

                    return attendeeIds;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public List<Project> GetAllProjects(int userId)
        {
            List<Project> allProjects = new List<Project>();
            try
            {
                _connection.Open();
                var queryString = "SELECT [Project].ProjectId, [Project].Name, [Project].Description, [Project].Difficulty, [Project].ManagerUserId, " +
                  "[Users].Name, [Project].CreatedDate, [Project].CreatedBy, [Project].EndDate, [Project].Completed FROM [Project] " +
                  "JOIN [Users] ON [Project].[ManagerUserId] = [Users].Id " +
                  "JOIN [ProjectPerson] ON [Project].ProjectId = [ProjectPerson].ProjectId " +
                  "WHERE [ProjectPerson].UserId = @USERID";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        Project currentProject = new Project();
                        currentProject.ProjectId = dbReader.GetInt32(0);
                        currentProject.Name = dbReader.GetString(1);
                        currentProject.Description = dbReader.GetString(2);
                        currentProject.Difficulty = dbReader.GetInt32(3);
                        currentProject.ManagerUserId = dbReader.GetInt32(4);
                        currentProject.ManagerUserName = dbReader.GetString(5);
                        currentProject.CreatedDate = dbReader.GetDateTime(6);
                        currentProject.CreatedDateStr = dbReader.GetDateTime(6).ToString("dd/MM/yyyy");
                        currentProject.CreatedBy = dbReader.GetInt32(7);
                        currentProject.EndDate = dbReader.IsDBNull(8) ? DateTime.MinValue : dbReader.GetDateTime(8);
                        currentProject.EndDateStr = dbReader.IsDBNull(8) ? "" : dbReader.GetDateTime(8).ToString("dd/MM/yyyy");
                        currentProject.Completed = dbReader.GetBoolean(9);
                        allProjects.Add(currentProject);
                    }
                    _connection.Close();
                    return allProjects;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }
        public Project GetProjectsStatsById(int projectId)
        {
            Project toReturn = new Project();

            try
            {
                _connection.Open();
                var queryString = "SELECT COUNT(TaskId) FROM [ProjectTask] JOIN [Project] ON ProjectTask.ProjectId = Project.ProjectId WHERE [ProjectTask].ProjectId = @PROJECTID;";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@PROJECTID", projectId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        toReturn.ProjectId = projectId;
                        toReturn.TotalTasks = dbReader.GetInt32(0);
                    }
                    _connection.Close();
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }

            try
            {
                _connection.Open();
                var queryString = "SELECT COUNT(TaskId) FROM [ProjectTask] WHERE ProjectId = @PROJECTID AND Completed = @TRUE;";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@PROJECTID", projectId);
                command.Parameters.AddWithValue("@TRUE", true);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        toReturn.CompletedTasks = dbReader.GetInt32(0);
                    }
                    _connection.Close();
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
            return toReturn;

        }

        public List<Project> GetAllProjectsStats(int userId)
        {
            List<Project> allProjects = GetAllProjects(userId);
            List<Project> toReturn = new List<Project>();
            for (int i=0; i<allProjects.Count; i++)
            {
                try
                {
                    _connection.Open();
                    var queryString = "SELECT COUNT(TaskId) FROM [ProjectTask] WHERE [ProjectTask].ProjectId = @PROJECTID;";
                    SqlCommand command = new SqlCommand(queryString, _connection);
                    command.Parameters.AddWithValue("@PROJECTID", allProjects[i].ProjectId);
                    using (SqlDataReader dbReader = command.ExecuteReader())
                    {
                        while (dbReader.Read())
                        {
                            Project currentProject = new Project();
                            currentProject.ProjectId = allProjects[i].ProjectId;
                            currentProject.Name = allProjects[i].Name;
                            currentProject.TotalTasks = dbReader.GetInt32(0);
                            toReturn.Add(currentProject);
                        }
                        _connection.Close();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return null;
                }
            }
            for (int i=0; i<toReturn.Count; i++)
            {
                try
                {
                    _connection.Open();
                    var queryString = "SELECT COUNT(TaskId) FROM [ProjectTask] WHERE ProjectId = @PROJECTID AND Completed = @TRUE;";
                    SqlCommand command = new SqlCommand(queryString, _connection);
                    command.Parameters.AddWithValue("@PROJECTID", toReturn[i].ProjectId);
                    command.Parameters.AddWithValue("@TRUE", true);
                    using (SqlDataReader dbReader = command.ExecuteReader())
                    {
                        while (dbReader.Read())
                        {
                            Project currentProject = new Project();
                            toReturn[i].CompletedTasks = dbReader.GetInt32(0);
                        }
                    }
                    _connection.Close();

                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return null;
                }
            }
            return toReturn;
        }
        public List<Project> GetAllPMProjects(int userId)
        {
            List<Project> allProjects = new List<Project>();
            try
            {
                _connection.Open();
                var queryString = "SELECT [Project].ProjectId, [Project].Name, [Project].Description, [Project].Difficulty, [Project].ManagerUserId, " +
                  "[Project].CreatedDate, [Project].CreatedBy, [Project].EndDate, [Project].Completed FROM [Project] WHERE [Project].ManagerUserId = @USERID";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        Project currentProject = new Project();
                        currentProject.ProjectId = dbReader.GetInt32(0);
                        currentProject.Name = dbReader.GetString(1);
                        currentProject.Description = dbReader.GetString(2);
                        currentProject.Difficulty = dbReader.GetInt32(3);
                        currentProject.ManagerUserId = dbReader.GetInt32(4);
                        currentProject.CreatedDate = dbReader.GetDateTime(5);
                        currentProject.CreatedDateStr = dbReader.GetDateTime(5).ToString("dd/MM/yyyy");
                        currentProject.CreatedBy = dbReader.GetInt32(6);
                        currentProject.EndDate = dbReader.IsDBNull(7) ? DateTime.MinValue : dbReader.GetDateTime(7);
                        currentProject.EndDateStr = dbReader.IsDBNull(7) ? "" : dbReader.GetDateTime(7).ToString("dd/MM/yyyy");
                        currentProject.Completed = dbReader.GetBoolean(8);
                        allProjects.Add(currentProject);
                    }
                }
                _connection.Close();
                return allProjects;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public List<Project> GetAllProjectsByUserId(int userId)
        {
            List<Project> allProjects = new List<Project>();
            try
            {
                _connection.Open();
                var queryString = "SELECT [Project].ProjectId, [Project].Name, [Project].Description, [Project].Difficulty, [Project].ManagerUserId, " +
                  "[Project].CreatedDate, [Project].CreatedBy, [Project].EndDate, [Project].Completed FROM [Project] WHERE [Project].ManagerUserId = @USERID";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        Project currentProject = new Project();
                        currentProject.ProjectId = dbReader.GetInt32(0);
                        currentProject.Name = dbReader.GetString(1);
                        currentProject.Description = dbReader.GetString(2);
                        currentProject.Difficulty = dbReader.GetInt32(3);
                        currentProject.ManagerUserId = dbReader.GetInt32(4);
                        currentProject.CreatedDateStr = dbReader.GetDateTime(5).ToString("dd/MM/yyyy");
                        currentProject.CreatedBy = dbReader.GetInt32(6);
                        currentProject.EndDateStr = dbReader.GetDateTime(7).ToString("dd/MM/yyyy");
                        currentProject.Completed = dbReader.GetBoolean(8);
                        allProjects.Add(currentProject);
                    }
                    _connection.Close();
                }
                //for (var i = 0; i < allProjects.Count(); i++)
                //{
                //    allProjects.ElementAt(i).Persons = GetProjectPersonsById(allProjects.ElementAt(i).ProjectId);
                //}
                return allProjects;


            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public List<User> GetProjectPersonsById(int projectId) //this method gets all attendees WHO HAVE NOT REJECTED THE MEETING, if they reject the meeting their entry in MA table
                                                         //will be deleted hence won't be retrieved in this method
        {
            List<User> personNames = new List<User>();
            try
            {
                _connection.Open();
                var queryString = "SELECT U.Id, U.[Name] FROM Users U " +
                                  "JOIN ProjectPerson PP ON U.[Id] = PP.[UserId] " +
                                  "WHERE PP.[ProjectId] = @PROJECTID";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@PROJECTID", projectId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        if (dbReader.IsDBNull(0))
                        {
                            return personNames;
                        }
                        User currentUser = new User();
                        currentUser.Id = dbReader.GetInt32(0);
                        currentUser.Name = dbReader.GetString(1);
                        personNames.Add(currentUser);
                        
                    }
                    _connection.Close();
                    return personNames;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public List<User> GetOrderedProjectPersonsById(int projectId) //this method gets all attendees WHO HAVE NOT REJECTED THE MEETING, if they reject the meeting their entry in MA table
                                                               //will be deleted hence won't be retrieved in this method
        {
            List<User> personNames = GetProjectPersonsById(projectId);
            List<User> toReturn = new List<User>();
            List<int> numberOfTasks = new List<int>();
            foreach (User user in personNames)
            {
                try
                {
                    _connection.Open();
                    var queryString = "SELECT COUNT(TaskId) FROM [ProjectTask] WHERE UserId = @USERID";

                    SqlCommand command = new SqlCommand(queryString, _connection);
                    command.Parameters.AddWithValue("@USERID", user.Id);
                    using (SqlDataReader dbReader = command.ExecuteReader())
                    {
                        while (dbReader.Read())
                        {
                            if (dbReader.IsDBNull(0))
                            {
                                numberOfTasks.Add(0);
                                continue;
                            }
                            numberOfTasks.Add(dbReader.GetInt32(0));
                        }
                        _connection.Close();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return null;
                }
            }
            for (int i=0; i<numberOfTasks.Count; i++)
            {
                int index = numberOfTasks.IndexOf(numberOfTasks.Min());
                toReturn.Add(personNames[index]);
                numberOfTasks[index] = Int32.MaxValue;
            }
            return toReturn;
        }

        
        public List<ProjectTask> GetAllPMTasks(int projectId)
        {
            List<ProjectTask> allTasks = new List<ProjectTask>();
            try
            {
                _connection.Open();
                var queryString = "SELECT [ProjectTask].TaskId, [ProjectTask].TaskText, [ProjectTask].CreateDate, [ProjectTask].Deadline, [ProjectTask].Completed, [ProjectTask].UserId, [Users].Name, [ProjectTask].CompletedDate FROM [ProjectTask] " + 
                  "LEFT JOIN [Users] ON ProjectTask.UserId = Users.Id WHERE [ProjectTask].ProjectId = @PROJECTID";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@PROJECTID", projectId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        ProjectTask currentTask = new ProjectTask();
                        currentTask.TaskId = dbReader.GetInt32(0);
                        currentTask.TaskText = dbReader.GetString(1);
                        currentTask.CreateDate = dbReader.GetDateTime(2);
                        currentTask.Deadline = dbReader.GetDateTime(3);
                        currentTask.DeadlineStr = dbReader.GetDateTime(3).ToString("dd/MM/yyyy");
                        currentTask.Completed = dbReader.GetBoolean(4);
                        currentTask.UserId = dbReader.IsDBNull(5) ? -1 : dbReader.GetInt32(5);
                        currentTask.UserName = dbReader.IsDBNull(6) ? "" : dbReader.GetString(6);
                        currentTask.CompletedDate = dbReader.IsDBNull(7) ? DateTime.MinValue : dbReader.GetDateTime(7);
                        currentTask.CompletedDateStr =  currentTask.CompletedDate.ToString("dd/MM/yyyy");
                        allTasks.Add(currentTask);
                    }
                    _connection.Close();
                    if (allTasks.Count() != 0)
                    {
                        allTasks = allTasks.OrderBy(e => e.Deadline).ToList();
                    }
                    return allTasks;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }


        public bool AddNewTask(int projectId, string task, DateTime deadline, int userId)
        {
            Console.WriteLine(projectId);
            try
            {
                _connection.Open();
                var queryString = "";
                if (userId == 0)
                {
                    queryString = "INSERT INTO [ProjectTask] VALUES (@PROJECTID, @TASK, @CURRENTDATE, @DEADLINE, @FALSE, NULL, NULL);";
                }
                else
                {
                    queryString = "INSERT INTO [ProjectTask] VALUES (@PROJECTID, @TASK, @CURRENTDATE, @DEADLINE, @FALSE, NULL, @USERID);";

                }
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@PROJECTID", projectId);
                command.Parameters.AddWithValue("@TASK", task);
                command.Parameters.AddWithValue("@CURRENTDATE", DateTime.Now);
                command.Parameters.AddWithValue("@DEADLINE", deadline);
                command.Parameters.AddWithValue("@FALSE", false);
                command.Parameters.AddWithValue("@USERID", userId);
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


        public bool DeleteTask(int taskId) //will have to update all tables so that instead of deleting you set Active to 0
        {
            try
            {
                _connection.Open();
                var queryString = "DELETE FROM [ProjectTask] WHERE TaskId = @TASKID;";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@TASKID", taskId);
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

        public ProjectTask GetTaskById(int taskId)
        {
            ProjectTask currentTask = new ProjectTask();
            try
            {
                _connection.Open();
                var queryString = "SELECT [ProjectTask].TaskId, [ProjectTask].TaskText, [ProjectTask].Deadline, [ProjectTask].Completed, [ProjectTask].UserId, [Users].Name, [ProjectTask].CompletedDate FROM [ProjectTask] " +
                  "LEFT JOIN [Users] ON ProjectTask.UserId = Users.Id WHERE [ProjectTask].TaskId = @TASKID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@TASKID", taskId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        currentTask.TaskId = dbReader.GetInt32(0);
                        currentTask.TaskText = dbReader.GetString(1);
                        currentTask.DeadlineStr = dbReader.GetDateTime(2).ToString("dd/MM/yyyy");
                        currentTask.Deadline = dbReader.GetDateTime(2);
                        currentTask.Completed = dbReader.GetBoolean(3);
                        currentTask.UserId = dbReader.IsDBNull(4) ? -1 : dbReader.GetInt32(4);
                        currentTask.UserName = dbReader.IsDBNull(5) ? "" : dbReader.GetString(5);
                        currentTask.CompletedDate = dbReader.IsDBNull(6) ? DateTime.MinValue : dbReader.GetDateTime(6);
                        currentTask.CompletedDateStr = currentTask.CompletedDate.ToString("dd/MM/yyyy");

                    }
                    _connection.Close();
                    return currentTask;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public bool ToggleCompleteTask(int taskId, bool complete)
        {
            try
            {
                var queryString = "";
                _connection.Open();
                if (complete == true)
                {
                    queryString = "UPDATE [ProjectTask] SET Completed = @BOOL, CompletedDate = @COMPLETEDDATE WHERE TaskId = @TASKID;";
                }
                else
                {
                    queryString = "UPDATE [ProjectTask] SET Completed = @BOOL, CompletedDate = NULL WHERE TaskId = @TASKID;";
                }

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@TASKID", taskId);
                command.Parameters.AddWithValue("@BOOL", complete);
                command.Parameters.AddWithValue("@COMPLETEDDATE", DateTime.Now);
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

        public bool UpdateTaskMethodPM(int updateTaskId, string updateTask, DateTime updateDeadline, int updateUser)
        {
            Console.WriteLine("update user is " + updateUser);
            try
            {
                _connection.Open();
                var queryString = "";
                if (updateUser != 0)
                {
                    queryString = "UPDATE [ProjectTask] SET TaskText = @TASKTEXT, Deadline = @DEADLINE, UserId = @USERID WHERE TaskId = @TASKID;";

                }
                else
                {
                    queryString = "UPDATE [ProjectTask] SET TaskText = @TASKTEXT, Deadline = @DEADLINE, UserId = NULL WHERE TaskId = @TASKID;";

                }

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@TASKTEXT", updateTask);
                command.Parameters.AddWithValue("@DEADLINE", updateDeadline);
                command.Parameters.AddWithValue("@TASKID", updateTaskId);
                command.Parameters.AddWithValue("@USERID", updateUser);
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

        public bool UpdateTaskMethod(int updateTaskId, string updateTask)
        {
            try
            {
                _connection.Open();
                var queryString = "UPDATE [ProjectTask] SET TaskText = @TASKTEXT WHERE TaskId = @TASKID;";


                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@TASKTEXT", updateTask);
                command.Parameters.AddWithValue("@TASKID", updateTaskId);
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


        public bool AllocateTask(int taskId, int userId)
        {
            try
            {
                _connection.Open();
                var queryString = "UPDATE [ProjectTask] SET UserId = @USERID WHERE TaskId = @TASKID;";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@TASKID", taskId);
                command.Parameters.AddWithValue("@USERID", userId);
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

        public bool AddNewComment(int userId, int projectId, string comment)
        {
            try
            {
                _connection.Open();
                var queryString = "INSERT INTO [EmployeeComment] VALUES (@USERID, @PROJECTID, @COMMENT, @COMMENTDATE);";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@PROJECTID", projectId);
                command.Parameters.AddWithValue("@COMMENT", comment);
                command.Parameters.AddWithValue("@COMMENTDATE", DateTime.Now);
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

        public List<ProjectTask> GetAllTasks(int userId)
        {
            List<ProjectTask> allTasks = new List<ProjectTask>();
            try
            {
                _connection.Open();
                var queryString = "SELECT [ProjectTask].TaskId, [ProjectTask].TaskText, [ProjectTask].CreateDate, [ProjectTask].Deadline, [ProjectTask].Completed, [Project].Name, [ProjectTask].CompletedDate FROM [ProjectTask] " +
                  "JOIN [Project] ON ProjectTask.ProjectId = Project.ProjectId WHERE [ProjectTask].UserId = @USERID";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        ProjectTask currentTask = new ProjectTask();
                        currentTask.TaskId = dbReader.GetInt32(0);
                        currentTask.TaskText = dbReader.GetString(1);
                        currentTask.CreateDate = dbReader.GetDateTime(2);
                        currentTask.Deadline = dbReader.GetDateTime(3);
                        currentTask.DeadlineStr = dbReader.GetDateTime(3).ToString("dd/MM/yyyy");
                        currentTask.Completed = dbReader.GetBoolean(4);
                        currentTask.ProjectName = dbReader.GetString(5);
                        currentTask.CompletedDate = dbReader.IsDBNull(6) ? DateTime.MinValue : dbReader.GetDateTime(6);
                        currentTask.CompletedDateStr = currentTask.CompletedDate.ToString("dd/MM/yyyy");
                        allTasks.Add(currentTask);
                    }
                    _connection.Close();
                    allTasks = allTasks.OrderBy(e => e.Deadline).ToList();
                    return allTasks;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public bool ToggleCompleteProject(int projectId, bool complete)
        {
            try
            {
                var queryString = "";
                _connection.Open();
                Console.WriteLine(complete);
                if (complete == true)
                {
                    queryString = "UPDATE [Project] SET EndDate = @COMPLETEDDATE, Completed = @BOOL WHERE ProjectId = @PROJECTID;";
                }
                else
                {
                    queryString = "UPDATE [Project] SET EndDate = NULL, Completed = @BOOL WHERE ProjectId = @PROJECTID;";
                }

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@PROJECTID", projectId);
                command.Parameters.AddWithValue("@BOOL", complete);
                command.Parameters.AddWithValue("@COMPLETEDDATE", DateTime.Now);
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
        public bool GetProjectStatus(int projectId)
        {
            List<ProjectTask> allTasks = new List<ProjectTask>();
            bool toReturn = false;
            try
            {
                _connection.Open();
                var queryString = "SELECT Completed FROM [Project] WHERE ProjectId = @PROJECTID;";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@PROJECTID", projectId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        toReturn = dbReader.GetBoolean(0);
                    }
                    _connection.Close();
                    return toReturn;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }
    }

}

