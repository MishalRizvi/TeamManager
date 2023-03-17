using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net.Mail;
using WebAppsNoAuth.Data;
using WebAppsNoAuth.Models;

namespace WebAppsNoAuth.Providers
{
	public class EmailProvider
	{
        private readonly SqlConnection _connection;
        private readonly IConfiguration _configuration;
        private readonly WebAppsNoAuthDbContext _webApps;        

        public EmailProvider(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
        {
            _webApps = webApps;
            _configuration = configuration;
            _connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb"));

        }

        public bool SendNewRequestEmail(int userId, int requestTypeId, DateTime startDate, DateTime endDate, string userName, string userEmail)
        {
            EmailTemplate emailTemplate = new EmailTemplate();
            emailTemplate.Subject = "New Request";
            var requestTypeName = "";

            //Direct mapping of request type id to string done here:
            if (requestTypeId == 1)
            {
                requestTypeName = "Annual";
            }
            if (requestTypeId == 2)
            {
                requestTypeName = "Study";
            }
            if (requestTypeId == 3)
            {
                requestTypeName = "WFH";
            }
            emailTemplate.Body = "Hi " + userName + "," + Environment.NewLine +
                                 "You have submitted a new request." + Environment.NewLine +
                                 "Request Type: " + requestTypeName + Environment.NewLine +
                                 "Start Date: " + startDate.ToString("dd/MM/yyyy") + Environment.NewLine +
                                 "End Date: " + endDate.ToString("dd/MM/yyyy") + Environment.NewLine;
            emailTemplate.To = userEmail;
            emailTemplate.ToName = userName;
            var success = SendEmail(emailTemplate);
            if (success)
            {
                return true;
            }
            return false;
        }

        public bool SendNewRequestEmailToManagers(int userId, int requestTypeId, DateTime startDate, DateTime endDate, string userName, int managerId, string managerName, string managerEmail)
        {
            EmailTemplate emailTemplate = new EmailTemplate();
            emailTemplate.Subject = "New Request";

            if (managerId == -1)
            {
                return true;
            }

            var requestTypeName = "";

            //Direct mapping of request type id to string done here:
            if (requestTypeId == 1)
            {
                requestTypeName = "Annual";
            }
            if (requestTypeId == 2)
            {
                requestTypeName = "Study";
            }
            if (requestTypeId == 3)
            {
                requestTypeName = "WFH";
            }
            emailTemplate.Body = "Hi " + managerName + "," + Environment.NewLine +
                                 userName + " has submitted a new request." + Environment.NewLine +
                                 "Request Type: " + requestTypeName + Environment.NewLine +
                                 "Start Date: " + startDate.ToString("dd/MM/yyyy") + Environment.NewLine +
                                 "End Date: " + endDate.ToString("dd/MM/yyyy") + Environment.NewLine + Environment.NewLine +
                                 "Please go to the Managers page to Approve or Reject this request";

            emailTemplate.To = managerEmail;
            emailTemplate.ToName = managerName;
            var success = SendEmail(emailTemplate);
            if (success)
            {
                return true;
            }
            return false;
        }

        public bool SendApproveRequestEmail(int userId, string requestTypeName, string requestStartDate, string requestEndDate, string userName, string userEmail, int managerId, string managerName)
        {
            EmailTemplate emailTemplate = new EmailTemplate();
            emailTemplate.Subject = "Request Approved";

            emailTemplate.Body = "Hi " + userName + "," + Environment.NewLine +
                                 "Your request:" + Environment.NewLine +
                                 "Request Type: " + requestTypeName + Environment.NewLine +
                                 "Start Date: " + requestStartDate + Environment.NewLine +
                                 "End Date: " + requestEndDate + Environment.NewLine +
                                 "has been approved by " + managerName + ".";

            emailTemplate.To = userEmail;
            emailTemplate.ToName = userName;
            var success = SendEmail(emailTemplate);
            if (success)
            {
                return true;
            }
            return false;
        }

        public bool SendRejectRequestEmail(int userId, string requestTypeName, string requestStartDate, string requestEndDate, string userName, string userEmail, int managerId, string managerName)
        {
            EmailTemplate emailTemplate = new EmailTemplate();
            emailTemplate.Subject = "Request Rejected";

            emailTemplate.Body = "Hi " + userName + "," + Environment.NewLine +
                                 "Your request:" + Environment.NewLine +
                                 "Request Type: " + requestTypeName + Environment.NewLine +
                                 "Start Date: " + requestStartDate + Environment.NewLine +
                                 "End Date: " + requestEndDate + Environment.NewLine +
                                 "has been rejected by " + managerName + ".";

            emailTemplate.To = userEmail;
            emailTemplate.ToName = userName;
            var success = SendEmail(emailTemplate);
            if (success)
            {
                return true;
            }
            return false;
        }

        public void SendMeetingInviteEmail(int userId, string userName, string userEmail, string hostName, string title, DateTime meetingDate, string startTime, string endTime)
        {
            EmailTemplate emailTemplate = new EmailTemplate();
            emailTemplate.Subject = "Meeting invite";


            emailTemplate.Body = "Hi " + userName + "," + Environment.NewLine +
                                 "You have been invited to a new meeting." + Environment.NewLine + Environment.NewLine +
                                 "Meeting Details:" + Environment.NewLine +
                                 "Date: " + meetingDate.ToString("dd/MM/yyyy") + Environment.NewLine +
                                 "Title: " + title + Environment.NewLine +
                                 "Host: " + hostName + Environment.NewLine + 
                                 "Start Time: " + startTime + Environment.NewLine +
                                 "End Time: " + endTime + Environment.NewLine + Environment.NewLine +
                                 "Please go to the Meetings page to Approve or Reject this invite.";

            emailTemplate.To = userEmail;
            emailTemplate.ToName = userName;

            var success = SendEmail(emailTemplate); //chosen to not return anything for this method, can change in the future
        }

        public void SendAcceptMeetingEmailToHost(string meetingTitle, string hostName, string hostEmail, string attendeeName)
        {
            EmailTemplate emailTemplate = new EmailTemplate();
            emailTemplate.Subject = "Update on your meeting: " + meetingTitle;


            emailTemplate.Body = "Hi " + hostName + "," + Environment.NewLine + Environment.NewLine +
                                 attendeeName + " has accepted the meeting: " + meetingTitle + Environment.NewLine;

            emailTemplate.To = hostEmail;
            emailTemplate.ToName = hostName;

            var success = SendEmail(emailTemplate); //chosen to not return anything for this method, can change in the future
        }

        public void SendRejectMeetingEmailToHost(string meetingTitle, string hostName, string hostEmail, string attendeeName)
        {
            EmailTemplate emailTemplate = new EmailTemplate();
            emailTemplate.Subject = "Update on your meeting: " + meetingTitle;


            emailTemplate.Body = "Hi " + hostName + "," + Environment.NewLine + Environment.NewLine +
                                 attendeeName + " has declined the meeting: " + meetingTitle + Environment.NewLine;

            emailTemplate.To = hostEmail;
            emailTemplate.ToName = hostName;

            var success = SendEmail(emailTemplate); //chosen to not return anything for this method, can change in the future
        }


        public void SendCancelMeetingEmail(string meetingTitle, string attendeeName, string attendeeEmail, string hostName)
        {
            EmailTemplate emailTemplate = new EmailTemplate();
            emailTemplate.Subject = "Meeting Cancellation";


            emailTemplate.Body = "Hi " + attendeeName + "," + Environment.NewLine + Environment.NewLine +
                                 hostName + " has cancelled the meeting: " + meetingTitle + Environment.NewLine;

            emailTemplate.To = attendeeEmail;
            emailTemplate.ToName = attendeeName;

            var success = SendEmail(emailTemplate); //chosen to not return anything for this method, can change in the future
        }

        public bool SendEmail(EmailTemplate emailTemplate)
        {

            try
            {
                MailMessage m = new MailMessage();
                SmtpClient sc = new SmtpClient();
                m.From = new MailAddress(_configuration.GetSection("EmailConfiguration")["From"], "ManaTeam");
                m.To.Add(new MailAddress(emailTemplate.To, emailTemplate.ToName));
                m.Subject = emailTemplate.Subject;
                m.Body = emailTemplate.Body;
                m.IsBodyHtml = false;
                sc.Host = "smtp.gmail.com";
                sc.Port = 587;
                sc.UseDefaultCredentials = false;
                sc.Credentials = new System.Net.NetworkCredential(_configuration.GetSection("EmailConfiguration")["From"], (_configuration.GetSection("EmailConfiguration")["Password"])); //change
                sc.EnableSsl = true; // runtime encrypt the SMTP communications using SSL
                sc.Send(m);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Cannot send email");
                Debug.WriteLine(e);
            }

            return false;
        }
    }
}

