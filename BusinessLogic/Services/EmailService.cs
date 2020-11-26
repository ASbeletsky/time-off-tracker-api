using BusinessLogic.Services.Interfaces;
using BusinessLogic.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
        {
            _smtpSettings = smtpSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = subject;
                message.Body = new TextPart(TextFormat.Html)
                {
                    Text = body
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, _smtpSettings.SSL);
                    await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in method: SendEmailAsync");
            }
        }
    }
}
