using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Domain.Enums;

namespace AppointmentSystem.Application.DTOS.Notification
{
    public class NotificationTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public NotificationType Type { get; set; }
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public int HoursBeforeAppointment { get; set; }
        public bool IsActive { get; set; }
    }
}
