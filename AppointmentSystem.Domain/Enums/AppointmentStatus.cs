using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentSystem.Domain.Enums
{
    public enum AppointmentStatus
    {
        Scheduled = 1,      // Programado
        Confirmed = 2,      // Confirmado
        InProgress = 3,     // En curso
        Completed = 4,      // Completado
        NoShow = 5,         // No asistió
        Cancelled = 6       // Cancelado
    }
}
