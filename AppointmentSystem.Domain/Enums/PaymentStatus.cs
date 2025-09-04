using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentSystem.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 1,        // Pendiente
        Paid = 2,           // Pagado
        Refunded = 3        // Reembolsado
    }
}
