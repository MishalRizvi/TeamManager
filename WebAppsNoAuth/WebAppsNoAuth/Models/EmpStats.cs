using System;
namespace WebAppsNoAuth.Models
{
	public class EmpStats
	{
		public EmpStats()
		{
		}

		public int UserId { get; set; }
		public int TaskDuration { get; set; }
        public int DeadlinesMissed { get; set; }
        public int ProjectsWorked { get; set; }
        public int ProjectsLed { get; set; }
		public float SatisfactionRate { get; set; }
	}
}

