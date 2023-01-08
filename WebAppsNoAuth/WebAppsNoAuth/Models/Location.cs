using System;
namespace WebAppsNoAuth.Models
{
	public class Location
	{
		public Location()
		{
		}
		public int LocationId { get; set; }
		public string LocationTitle { get; set; }
		public string LocationName { get; set; }
		public int InstitutionId { get; set; }
	}
}

