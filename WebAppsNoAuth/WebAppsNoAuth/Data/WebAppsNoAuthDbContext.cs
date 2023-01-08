using System;
using Microsoft.EntityFrameworkCore;
using WebAppsNoAuth.Models;
namespace WebAppsNoAuth.Data
{
	public class WebAppsNoAuthDbContext : DbContext
	{
        public WebAppsNoAuthDbContext(DbContextOptions<WebAppsNoAuthDbContext> options) : base(options)
		{
		}
		public DbSet<Company> Company { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<Location> Location { get; set; }
		public DbSet<Team> Team { get; set; }
		//public DbSet<TeamUser> TeamUser { get; set; }
        public DbSet<Request> Request { get; set; }
	//	public DbSet<UserEntitlements> UserEntitlements { get; set; }
    }
}

