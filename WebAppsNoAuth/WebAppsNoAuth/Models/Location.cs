using System;
namespace WebAppsNoAuth.Models
{
	public class Location
	{
		public Location()
		{
		}
		public int LocationId { get; set; }
        public string LocationValue { get; set; }
        public string LocationTitle { get; set; }
		public int InstitutionId { get; set; }
	}
}

