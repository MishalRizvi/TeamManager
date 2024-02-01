using System;
namespace WebAppsNoAuth.Models
{
    public class ProjectTask
    {
        public ProjectTask()
        {
        }

        public int TaskId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string TaskText { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime Deadline { get; set; }
        public string DeadlineStr { get; set; }
        public bool Completed { get; set; }
        public DateTime CompletedDate { get; set; }
        public string CompletedDateStr { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }

    }
}

