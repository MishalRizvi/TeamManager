using System;
namespace WebAppsNoAuth.Models
{
	public class Request
	{
		public Request()
		{
		}

		public int RequestId { get; set; }
		public int RequestTypeId { get; set; }
		public int UserId { get; set; }
		public string UserName { get; set; }
		public DateTime StartDate { get; set; }
		public string StartDateStr { get; set; }
        public DateTime EndDate { get; set; }
        public string EndDateStr { get; set; }
        public string RequestTypeName { get; set; }
        public Boolean Approved { get; set; }
        public string ApprovedMessage { get; set; }
    }
}

