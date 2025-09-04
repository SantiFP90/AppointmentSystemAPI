using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentSystem.Application.DTOS.User
{
    public class UserDto
    {
        public string FullName { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public int? Age { get; set; }

        public string DNI { get; set; } = null!;

        public string Email { get; set; } = null!;

        public int? RoleId { get; set; }

        public string RoleName { get; set; } = null!;
    }
}
