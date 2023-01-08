//using System;
//using MailKit.Security;
//using Microsoft.Extensions.Options;
//using MimeKit;
//using WebAppsNoAuth.Models;

//namespace WebAppsNoAuth.Services
//{
//	public class EmailService
//	{
//        private readonly Email _mail;

//        public EmailService(IOptions<Email> mail)
//		{
//            _mail = mail.Value;
//		}

//        public async Task SendEmailAsync(MailRequest mailRequest)
//        {
//            var email = new MimeMessage();
//            email.Sender = MailboxAddress.Parse(_mail.From);
//            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
//            email.Subject = mailRequest.Subject;
//            var builder = new BodyBuilder();
//            if (mailRequest.Attachments != null)
//            {
//                byte[] fileBytes;
//                foreach (var file in mailRequest.Attachments)
//                {
//                    if (file.Length > 0)
//                    {
//                        using (var ms = new MemoryStream())
//                        {
//                            file.CopyTo(ms);
//                            fileBytes = ms.ToArray();
//                        }
//                        builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
//                    }
//                }
//            }
//            builder.HtmlBody = mailRequest.Body;
//            email.Body = builder.ToMessageBody();
//            using var smtp = new SmtpClient();
//            smtp.Connect(_mail.SmtpServer, _mail.Port, SecureSocketOptions.StartTls);
//            smtp.Authenticate(_mail.From, _mail.Password);
//            await smtp.SendAsync(email);
//            smtp.Disconnect(true);
//        }
//    }
//}

