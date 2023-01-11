using System;
using System.Data.SqlClient;
using System.Diagnostics;
using WebAppsNoAuth.Data;
using WebAppsNoAuth.Providers;

namespace WebAppsNoAuth.Controllers
{
	public class ProviderController
	{
		public UserProvider User; 
		public EmailProvider Email; 
		public ManagerProvider Manager;
        public LocationProvider Location;
        public TeamProvider Team;
        public RequestProvider Request;

        private readonly WebAppsNoAuthDbContext _webApps;
        private readonly IConfiguration _configuration;

        public ProviderController(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
        {
            _webApps = webApps;
            _configuration = configuration;

          //  SqlConnection pconnection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb"));
            User = new UserProvider(_webApps,_configuration);
            Email = new EmailProvider(_webApps,_configuration); //needs config
            Manager = new ManagerProvider(_webApps, _configuration);
            Location = new LocationProvider(_webApps, _configuration);
            Team = new TeamProvider(_webApps, _configuration);
            Request = new RequestProvider(_webApps, _configuration);
        }
    }
}

