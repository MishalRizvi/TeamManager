using System;
namespace WebAppsNoAuth.Models
{
    public class Meeting
    {
        public Meeting()
        {
        }

        public int MeetingId { get; set; }
        public string Title { get; set; }
        public int HostUserId { get; set; }
        public string HostUserName { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string MeetingDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool Active { get; set; }
        public string Attendees { get; set; }
    }
}

