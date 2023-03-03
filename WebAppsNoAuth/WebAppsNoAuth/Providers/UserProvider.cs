using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities.Zlib;
using WebAppsNoAuth.Data;
using WebAppsNoAuth.Models;

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
                var queryString = "SELECT U.[Id], U.[Name], U.[Email], U.[Password], U.[ContactNumber], U.[ManagerUserId], U2.[Name], U.[Admin], U.[Manager] FROM [Users] U " +
                                  "LEFT JOIN [Users] U2 ON U.[ManagerUserId] = U2.[Id] WHERE U.[InstitutionId] = @INSTITUTIONID"; //Left join to allow retrieval of users with null ManagerUserId 
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

        public List<User> GetAllUsersAsObject(int userId)
        {
            var institutionId = GetInstitutionId(userId);

            List<User> allUsers = new List<User>();
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
                        allUsers.Add(currentUser);
                    }
                    _connection.Close();
                }
                return allUsers;
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
               // var passwordHashed = HashPassword(password);
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
            try
            {
                _connection.Open();
                var queryString = "";
                if (locationId != -1)
                {
                    queryString = "UPDATE [Status] SET StatusTypeId = @STATUSTYPEID, CurrentLocationId = @LOCATIONID, WFH = @ISWFH, WFHContact = @WFHCONTACT WHERE UserId = @USERID";
                }
                else
                {
                    queryString = "UPDATE [Status] SET StatusTypeId = @STATUSTYPEID, CurrentLocationId = NULL, WFH = @ISWFH, WFHContact = @WFHCONTACT WHERE UserId = @USERID";
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

                var queryString = "DELETE FROM [UserEntitlements] WHERE UserId = @USERID;" +
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
    }
}

