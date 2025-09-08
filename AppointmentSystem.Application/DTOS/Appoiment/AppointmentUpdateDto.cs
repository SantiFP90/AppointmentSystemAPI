using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Domain.Enums;

namespace AppointmentSystem.Application.DTOS.Appoiment
{
    public class AppointmentUpdateDto
    {
        public AppointmentStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public int? TimeSlotId { get; set; }
        public string? Notes { get; set; }
        public decimal? Amount { get; set; }
    }
}
