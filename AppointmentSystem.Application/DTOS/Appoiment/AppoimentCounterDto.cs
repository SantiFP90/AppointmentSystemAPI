using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentSystem.Application.DTOS.Appoiment
{
    public class AppoimentCounterDto
    {
        public int AvailableAppointments { get; set; }
        public int PedingAppointmentsToday { get; set; }
        public int PendingAppoimentsThisWeek { get; set; }

    }
}
