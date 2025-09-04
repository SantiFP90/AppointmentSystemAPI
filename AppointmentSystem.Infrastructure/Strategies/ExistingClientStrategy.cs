using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Application.DTOS.Appoiment;
using AppointmentSystem.Application.Interfaces.Strategies;

namespace AppointmentSystem.Infrastructure.Strategies
{
    public class ExistingClientStrategy : IClientCreationStrategy
    {
        public Task<int> GetOrCreateClientIdAsync(AppointmentCreateDto dto)
        {
            if (dto.ClientId == null || dto.ClientId == 0)
                throw new InvalidOperationException("ClientId es requerido para esta estrategia.");

            return Task.FromResult(dto.ClientId);
        }
    }
}
