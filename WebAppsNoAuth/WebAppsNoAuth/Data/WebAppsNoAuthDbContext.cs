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
	}
}

