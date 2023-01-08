using System;
namespace WebAppsNoAuth.Models
{
	public class Team
	{
		public Team()
		{
		}

		public int TeamId { get; set; }
		public string TeamName { get; set; }
		public int LocationId { get; set; }
    }
}

