using AppointmentSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentSystem.Application.DTOS.Appoiment
{
    public class AppoimentMonthDto
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = null!;
        public string ClientEmail { get; set; } = null!;
        public string ClientPhoneNumber { get; set; } = null!;
        public string? Notes { get; set; }
        public AppointmentStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal? Amount { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
