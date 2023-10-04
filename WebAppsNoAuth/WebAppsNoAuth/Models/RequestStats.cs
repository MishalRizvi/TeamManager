using System;
namespace WebAppsNoAuth.Models
{
    public class RequestStats
    {
        public RequestStats()
        {
        }

        public int UserId { get; set; }
        public int EntitlementAmount { get; set; }
        public int AnnualUsed { get; set; }
        public int WFHUsed { get; set; }
        public int StudyUsed { get; set; }
        public int TotalUsed { get; set; }
    }
}

