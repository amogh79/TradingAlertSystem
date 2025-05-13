using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiNoticeProcessor.Helper
{
    public class AlertHelper : IAlertHelper
    {
        private readonly IConfiguration _config;
        public AlertHelper(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> SendEmail(string emailBody, List<string> toEmailAddresses)
        {
            try
            {
                MimeMessage email = new MimeMessage();

                email.From.Add(new MailboxAddress("TAS Alerts", "tasalerts2025@gmail.com"));

                foreach (string toEmailAddress in toEmailAddresses)
                {
                    email.To.Add(new MailboxAddress(toEmailAddress, toEmailAddress));
                }

                email.Subject = "Trading Alert System Alert";

                BodyBuilder bodyBuilder = new BodyBuilder();

                bodyBuilder.HtmlBody = emailBody;

                email.Body = bodyBuilder.ToMessageBody();

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    smtpClient.Connect("smtp.gmail.com", 587, false);
                    smtpClient.Authenticate(_config["TasEmail"], _config["TasEmailPassword"]);                    
                    await smtpClient.SendAsync(email);
                    smtpClient.Disconnect(true);
                }
                
                return true;
            }
            catch (Exception ex)
            {

                throw ex;                
            }            
        }
    }
}
