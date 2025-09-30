using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Application.DTOS.Appoiment;
using AppointmentSystem.Application.Interfaces.Repositories;
using AppointmentSystem.Application.Interfaces.Strategies;
using AppointmentSystem.Domain.Entities;

namespace AppointmentSystem.Infrastructure.Strategies
{
    public class NewUserStrategy : IClientCreationStrategy
    {
        private readonly IUnitOfWork _unitOfWork;

        public NewUserStrategy(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> GetOrCreateClientIdAsync(AppointmentCreateDto dto)
        {
            var userEntity = new User
            {
                FullName = dto.ClientName,
                PhoneNumber = dto.ClientPhoneNumber,
                Email = dto.ClientEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("defaultPassword123"),
                RoleId = 3,
            };

            await _unitOfWork.Users.Create(userEntity);
            await _unitOfWork.SaveChangesAsync();

            return userEntity.Id;
        }
    }
}
