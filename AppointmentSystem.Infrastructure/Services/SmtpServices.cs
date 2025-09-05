using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using AppointmentSystem.Application.Interfaces.Services;

namespace AppointmentSystem.Infrastructure.Services
{

    public class SmtpServices : ISmtpService
    {
        private readonly IConfiguration _configuration;

        public SmtpServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpHost = _configuration["SmtpSettings:Host"];
                var smtpPort = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
                var smtpUser = _configuration["SmtpSettings:Username"];
                var smtpPass = _configuration["SmtpSettings:Password"];
                var smtpFrom = _configuration["SmtpSettings:From"];


                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(smtpFrom!),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                mail.To.Add(to);

                await smtpClient.SendMailAsync(mail);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
