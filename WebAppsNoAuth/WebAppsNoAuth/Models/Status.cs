using System;
namespace WebAppsNoAuth.Models
{
    public class Status
    {
        public Status()
        {
        }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public int StatusTypeId { get; set; }
        public string StatusTypeName { get; set; }
        public int LocationId { get; set; }
        public string LocationValue { get; set; }
        public bool WFH { get; set; }
        public string WFHContact { get; set; }
    }
}

