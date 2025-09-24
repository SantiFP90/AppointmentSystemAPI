using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentSystem.Application.DTOS.Calendar
{
    public class CalendarDayDto
    {
        public int WorkingDayId { get; set; }
        public DateTime Date { get; set; }
        public List<CalendarTimeSlotDto> TimeSlots { get; set; } = new();
    }

}
