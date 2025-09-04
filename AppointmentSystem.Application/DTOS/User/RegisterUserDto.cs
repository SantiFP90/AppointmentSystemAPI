using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentSystem.Application.DTOS.User
{
    public class RegisterUserDto
    {
        public string FullName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public int? Age { get; set; }

        public string DNI { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public int RoleId { get; set; }
    }
}
