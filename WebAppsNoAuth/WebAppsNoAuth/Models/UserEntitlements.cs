using System;
namespace WebAppsNoAuth.Models
{
	public class UserEntitlements
	{
		public UserEntitlements()
		{
		}

		public int UserId { get; set; }
		public string Name { get; set; }
		public int Amount { get; set; }
		public int Year { get; set; }
    }
}

