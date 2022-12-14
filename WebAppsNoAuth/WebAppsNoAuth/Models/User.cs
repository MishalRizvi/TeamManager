using System;
namespace WebAppsNoAuth.Models
{
	public class User
	{
		public User()
		{
		}
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Admin { get; set; }
        public int InstitutionID { get; set; }
        public string Institution { get; set; }

    }
}

