using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Domain.Enums;

namespace AppointmentSystem.Domain.Entities
{
    public class NotificationLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        public int NotificationTemplateId { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        [MaxLength(200)]
        public string Recipient { get; set; } = null!; // Email o teléfono

        [Required]
        public string Subject { get; set; } = null!;

        [Required]
        public string Body { get; set; } = null!;

        public bool IsSent { get; set; } = false;
        public DateTime? SentAt { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; } = null!;

        [ForeignKey("NotificationTemplateId")]
        public NotificationTemplate NotificationTemplate { get; set; } = null!;
    }
}
