﻿using System;
namespace WebAppsNoAuth.Models
{
    public class Project
    {
        public Project()
        {
        }

        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Difficulty { get; set; }
        public int ManagerUserId { get; set; }
        public string ManagerUserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedDateStr { get; set; }
        public int CreatedBy { get; set; }
        public string Persons { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public DateTime EndDate { get; set; }
        public string EndDateStr { get; set; }
        public bool Completed { get; set; }

    }
}

