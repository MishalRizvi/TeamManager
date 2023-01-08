using System;
namespace WebAppsNoAuth.Models
{
	public class EmailTemplate
	{
		public EmailTemplate()
		{
		}

		public string Subject { get; set; }
		public string Body { get; set; }
		public string To { get; set; }
		public string ToName { get; set; }
	}
}

