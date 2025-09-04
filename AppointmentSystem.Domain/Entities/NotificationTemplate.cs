using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Domain.Enums;

namespace AppointmentSystem.Domain.Entities
{
    public class NotificationTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public string Subject { get; set; } = null!; // Para emails

        [Required]
        public string Body { get; set; } = null!;

        public int HoursBeforeAppointment { get; set; } // Ej: 24 horas antes

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
    }
}
