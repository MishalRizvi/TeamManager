using System;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using WebAppsNoAuth.Models;
using MailKit.Net.Smtp;

namespace WebAppsNoAuth.Controllers
{
	public class EmailController
	{
        private readonly IConfiguration _configuration;

        public EmailController(IConfiguration configuration)
		{
            _configuration = configuration;
        }

    public bool SendEmail(string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailConfiguration")["From"]));
            email.To.Add(MailboxAddress.Parse(_configuration.GetSection("EmailConfiguration")["From"])); //change 
            email.Subject = "subjectTest";
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };
            using var smtp = new SmtpClient();
            smtp.Connect(_configuration.GetSection("EmailConfiguration")["SmtpServer"], 465, SecureSocketOptions.StartTls);
            smtp.Authenticate("servicesappforyou@gmail.com", "AppService111");
            smtp.Send(email);
            smtp.Disconnect(true);

            return true;
        }
    }
}

