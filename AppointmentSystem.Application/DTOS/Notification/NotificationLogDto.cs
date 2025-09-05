using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Domain.Enums;

namespace AppointmentSystem.Application.DTOS.Notification
{
    public class NotificationLogDto
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public NotificationType Type { get; set; }
        public string Recipient { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
