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
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Required]
        public int TimeSlotId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ClientName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string ClientEmail { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string ClientPhoneNumber { get; set; } = null!;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public decimal? Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("ClientId")]
        public User Client { get; set; } = null!;

        [ForeignKey("TimeSlotId")]
        public TimeSlot TimeSlot { get; set; } = null!;

        public ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
    }

}
