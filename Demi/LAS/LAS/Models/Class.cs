//using MailKit.Net.Smtp;
//using MailKit.Security;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Configuration;
//using MimeKit;
//using MimeKit.Text;
//using RoyalKiddies.Application.Services.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Mail;
//using System.Text;
//using System.Threading.Tasks;
//using SmtpClient = MailKit.Net.Smtp.SmtpClient;
//using RoyalKiddies.Application.Dtos;
//using FluentEmail.Core;
//using FluentEmail.Razor;
//using FluentEmail.Smtp;
//using System.Net.Mail;
//using System.Text;
//using RoyalKiddies.Application.Services.Interfaces;
//using Microsoft.Extensions.Configuration;
//using LAS.Migrations;

////Service: Methods Implementation
//namespace RoyalKiddies.Application.Services.Implementation
//{
//    public class Alert : IAlert
//    {
//        private readonly IConfiguration _config;

//        public Alert(IConfiguration config)
//        {
//            _config = config;
//        }
//        public class EmailDto
//        {
//            public string To { get; set; } = string.Empty;
//            public string Subject { get; set; } = string.Empty;
//            public string Body { get; set; } = string.Empty;
//            public List<IFormFile>? Attachments { get; set; }
//        }
//        public async Task WardLoginAlertAsync(string emailto, string Name)
//        {
//            var email = new MimeMessage();
//            email.From.Add(MailboxAddress.Parse(_config.GetSection("EmailUsername").Value));
//            email.To.Add(MailboxAddress.Parse(emailto));
//            email.Subject = "Log In Notification";
//            email.Body = new TextPart(TextFormat.Plain)
//            {
//                Text = $"Hi {Name}, Please be informed that your Royal" +
//                $" Kiddies account was accessed on  {DateTime.Now}. \n If you did not log on to your account, " +
//                $"please let us know by sending an email to help@alat.ng. \n Alternatively, you can call " +
//                $"+23480000000 or 012000000 immediately. Thanks"
//            };


//            using var smtp = new SmtpClient();
//            smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
//            smtp.Authenticate(_config.GetSection("EmailUsername").Value, _config.GetSection("EmailPassword").Value);
//            await smtp.SendAsync(email);
//            smtp.Disconnect(true);
//        }

//        public async Task CreatedWardAlertAsync(string emailto, string Name, string password)
//        {
//            var email = new MimeMessage();
//            email.From.Add(MailboxAddress.Parse(_config.GetSection("EmailUsername").Value));
//            email.To.Add(MailboxAddress.Parse(emailto));
//            email.Subject = "Welcome to ALAT!";
//            email.Body = new TextPart(TextFormat.Plain)
//            {

//                Text = $"Hi {Name}, \n \n Welcome on Board! \n \n A Royal Kiddies account has been created for you!" + $"\n Here is your Username:{emailto} and  password: {password} \n Download the ALAT App and Login with these details"
//            };



//            using var smtp = new SmtpClient();
//            smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
//            smtp.Authenticate(_config.GetSection("EmailUsername").Value, _config.GetSection("EmailPassword").Value);
//            await smtp.SendAsync(email);
//            smtp.Disconnect(true);
//        }

//        public async Task WardAccountStatementAlertAsync(string emailto, string Name)
//        {
//            var email = new MimeMessage();
//            email.From.Add(MailboxAddress.Parse(_config.GetSection("EmailUsername").Value));
//            email.To.Add(MailboxAddress.Parse(emailto));
//            email.Subject = "Account Statement";
//            email.Body = new TextPart(TextFormat.Plain);

//            var builder = new BodyBuilder();
//            builder.Attachments.Add("WardAccountStatement.pdf");
//            builder.TextBody = "Dear " + Name + ",\n" + $"Your requested statement of account has been generated(see attached)  \n A password protection has been integrated to offer security and confidentiality. When a password request is made, Please enter the last 5 digits of your account number to access your account statement.\n \n \n" +
//                $"For any feedback or enquiries, please call our contact center on +2348000000 or send an email to help@alat.ng.";
//            email.Body = builder.ToMessageBody();


//            using var smtp = new SmtpClient();
//            smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
//            smtp.Authenticate(_config.GetSection("EmailUsername").Value, _config.GetSection("EmailPassword").Value);
//            await smtp.SendAsync(email);
//            smtp.Disconnect(true);
//        }

//        public async Task WardTransactionAlertAsync(string Name, string emailto, string Acct, decimal Amt, string Desc, decimal Balc)
//        {
//            string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\WardTransactionTemplate.html";
//            StreamReader str = new StreamReader(FilePath);
//            string MailText = str.ReadToEnd();
//            str.Close();
//            MailText = MailText.Replace("[Name]", Name).Replace("[Desc]", Desc)
//            .Replace("[Acct]", Acct).Replace("[Amt]", Amt.ToString()).Replace("[Balc]", Balc.ToString()).Replace("[Date]", DateTime.UtcNow.AddHours(1).ToString());

//            var email = new MimeMessage();
//            email.From.Add(MailboxAddress.Parse(_config.GetSection("EmailUsername").Value));
//            email.To.Add(MailboxAddress.Parse(emailto));
//            email.Subject = "Transaction Alert!";
//            var builder = new BodyBuilder();
//            builder.HtmlBody = MailText;
//            email.Body = builder.ToMessageBody();

//            using var smtp = new SmtpClient();
//            smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
//            smtp.Authenticate(_config.GetSection("EmailUsername").Value, _config.GetSection("EmailPassword").Value);
//            await smtp.SendAsync(email);
//            smtp.Disconnect(true);
//        }
//        public async Task ForgotPasswordNotificationAsync(string emailto, string Name, string DefaultPassword)
//        {
//            var email = new MimeMessage();
//            email.From.Add(MailboxAddress.Parse(_config.GetSection("EmailUsername").Value));
//            email.To.Add(MailboxAddress.Parse(emailto));
//            email.Subject = "Password reset Notification";
//            email.Body = new TextPart(TextFormat.Plain)
//            {
//                Text = $"Hi {Name}, \n \n There was a request to change your password! \n \n " +
//                $"If you did not make this request then please ignore this email.\n" +
//                $"Otherwise, please use this generated password: '{DefaultPassword}' to reset your password."
//            };



//            using var smtp = new SmtpClient();
//            smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
//            smtp.Authenticate(_config.GetSection("EmailUsername").Value, _config.GetSection("EmailPassword").Value);
//            await smtp.SendAsync(email);
//            smtp.Disconnect(true);
//        }
//        //Method that sends the debit alert
//        public async Task SendDebitNotificationAsync(Notification notification)
//        {
//            var sender = new FluentEmail.Smtp.SmtpSender(() => new System.Net.Mail.SmtpClient("smtp.gmail.com")
//            {
//                Credentials = new System.Net.NetworkCredential("wemaroyalkiddies@gmail.com", "bxhcvtamizvduaoz"),
//                EnableSsl = true,
//                DeliveryMethod = SmtpDeliveryMethod.Network,
//                Port = 587
//            });

//            StringBuilder template = new();
//            template.AppendLine("<p>TRANSACTION NOTIFICATION</p>");
//            template.AppendLine("Hi @Model.AccountName,");
//            template.AppendLine("<p>@Model.Amount has left your ALAT account.</p>");
//            template.AppendLine("<p>Account Number: @Model.AccountNumber.</p>");
//            template.AppendLine("<p>Account Balance: @Model.AccountBalance.</p>");
//            template.AppendLine("<p>Description: @Model.Description.</p>");
//            template.AppendLine("<p>Transaction Date: @Model.DateTime.</p>");
//            template.AppendLine("- The ALAT Team");

//            Email.DefaultSender = sender;
//            Email.DefaultRenderer = new RazorRenderer();
//            var email = await Email
//                .From("wemaroyalkiddies@gmail.com")
//                .To(notification.EmailAddress, notification.AccountName)
//                .Subject("Your Alat Account Has Been Debited")
//                //.AttachFromFilename($"{Directory.GetCurrentDirectory()}/properties/Files/samplePdf.pdf", "application/pdf", "Account Statement")
//                .UsingTemplateFromFile($"{Directory.GetCurrentDirectory()}\\Templates\\GuardianTransactionTemplate.cshtml",
//                new
//                {
//                    AccountName = notification.AccountName,
//                    Amount = notification.Amount,
//                    AccountNumber = notification.AccountNumber,
//                    AccountBalance = notification.AccountBalance,
//                    Description = notification.Description,
//                    TransactionDate = notification.TransactionDate


//                })
//                .SendAsync();

//        }

//        //method that sends the standing order alert  
//        public async Task SendStandingOrderNotificationAsync(Notification notification)

//        {
//            var sender = new FluentEmail.Smtp.SmtpSender(() => new System.Net.Mail.SmtpClient("smtp.gmail.com")
//            {
//                Credentials = new System.Net.NetworkCredential("wemaroyalkiddies@gmail.com", "bxhcvtamizvduaoz"),
//                EnableSsl = true,
//                DeliveryMethod = SmtpDeliveryMethod.Network,
//                Port = 25
//            });

//            StringBuilder template = new();
//            template.AppendLine("<p>TRANSACTION NOTIFICATION</p>");
//            template.AppendLine("Hi @Model.AccountName,");
//            template.AppendLine("<p>Your Standing Order was unsuccessful</p>");
//            template.AppendLine("<p>Amount: @Model.Amount.</p>");
//            template.AppendLine("<p>Status: failed</p>");
//            template.AppendLine("<p>Insufficient funds.</p>");
//            template.AppendLine("<p>Transaction Date: @Model.DateTime.</p>");
//            template.AppendLine("- The ALAT Team");

//            Email.DefaultSender = sender;
//            Email.DefaultRenderer = new RazorRenderer();
//            var email = await Email
//                .From("wemaroyalkiddies@gmail.com")
//                .To(notification.EmailAddress, notification.AccountName)
//                .Subject("Standing Order Unsuccessful")
//                .UsingTemplate(template.ToString(), new { AccountName = notification.AccountName, Amount = notification.Amount, DateTime = DateTime.Now })
//                .SendAsync();
//        }

//        public async Task ResetPinNotification(string emailto, string Name, string DefaultPin)
//        {
//            var email = new MimeMessage();
//            email.From.Add(MailboxAddress.Parse(_config.GetSection("EmailUsername").Value));
//            email.To.Add(MailboxAddress.Parse(emailto));
//            email.Subject = "Pin reset Notification";
//            email.Body = new TextPart(TextFormat.Plain)
//            {
//                Text = $"Hi {Name}, \n \n There was a request to change your Transaction Pin! \n \n " +
//                $"If you did not make this request then please ignore this email.\n" +
//                $"Otherwise, please use this generated Pin: '{DefaultPin}' to change your Pin."
//            };



//            using var smtp = new SmtpClient();
//            smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
//            smtp.Authenticate(_config.GetSection("EmailUsername").Value, _config.GetSection("EmailPassword").Value);
//            await smtp.SendAsync(email);
//            smtp.Disconnect(true);
//        }

//    }
//}

















