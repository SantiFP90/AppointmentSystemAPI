using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentSystem.Application.DTOS.Appoiment
{
    public class AppointmentCreateDto
    {
        public int ClientId { get; set; }
        public int TimeSlotId { get; set; }
        public string ClientName { get; set; } = null!;
        public string ClientEmail { get; set; } = null!;
        public string ClientPhoneNumber { get; set; } = null!;
        public string? Notes { get; set; }
        public decimal? Amount { get; set; }
    }
}
