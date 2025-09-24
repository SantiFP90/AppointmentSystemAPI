using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentSystem.Application.DTOS.WorkingDay
{
    public class WorkingDayByRangeDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public List<int> Days { get; set; } = new List<int>();

        public int SlotDurationMinutes { get; set; }
    }
}
