//using System;
//using System.Data.SqlClient;
//using WebAppsNoAuth.Data;

//namespace WebAppsNoAuth.Services
//{
//	public class WebService
//	{
//        private readonly WebAppsNoAuthDbContext _webApps;
//        private readonly IConfiguration _configuration;
//        public WebService(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
//        {
//            _webApps = webApps;
//            _configuration = configuration;
//        }
         
//        public static bool IsUserAdmin(int userId)
//        {
//            bool isAdmin = false;
//            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
//            {
//                connection.Open();
//                var queryString = "SELECT Admin FROM [Users] WHERE Id = @ID";
//                SqlCommand command = new SqlCommand(queryString, connection);
//                command.Parameters.AddWithValue("@ID", userId);
//                using (SqlDataReader dbReader = command.ExecuteReader())
//                {
//                    while (dbReader.Read())
//                    {
//                        if (dbReader.IsDBNull(0))
//                        {
//                            return isAdmin;
//                        }
//                        isAdmin = dbReader.GetBoolean(0);
//                    }
//                }
//                return isAdmin;
//            }
//        }

//        public static int GetInstitutionId(int userId)
//        {
//            int institutionId = -1;
//            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb")))
//            {
//                connection.Open();
//                var queryString = "SELECT InstitutionID FROM [Users] WHERE Id = @USERID";
//                SqlCommand command = new SqlCommand(queryString, connection);
//                command.Parameters.AddWithValue("@USERID", userId);
//                using (SqlDataReader dbReader = command.ExecuteReader())
//                {
//                    while (dbReader.Read())
//                    {
//                        if (dbReader.IsDBNull(0))
//                        {
//                            return institutionId;
//                        }
//                        institutionId = dbReader.GetInt32(0);
//                    }
//                }
//                return institutionId;
//            }
//        }

//    }
//}

