using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentSystem.Application.Interfaces.Services
{
    public interface ISmtpService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    }
}
