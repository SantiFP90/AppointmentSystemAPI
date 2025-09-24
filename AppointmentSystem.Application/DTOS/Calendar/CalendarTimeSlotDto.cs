using AppointmentSystem.Application.DTOS.Appoiment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentSystem.Application.DTOS.Calendar
{
    public class CalendarTimeSlotDto
    {
            public TimeSpan StartTime { get; set; }           
            public TimeSpan EndTime { get; set; }             
            public bool IsAvailable { get; set; }            
            public AppointmentDto? Appointment { get; set; } 
    }
}
