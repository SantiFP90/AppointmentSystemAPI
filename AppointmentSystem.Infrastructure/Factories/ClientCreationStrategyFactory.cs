using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Application.DTOS.Appoiment;
using AppointmentSystem.Application.Interfaces.Fatories;
using AppointmentSystem.Application.Interfaces.Strategies;
using AppointmentSystem.Infrastructure.Strategies;

namespace AppointmentSystem.Infrastructure.Factories
{
    public class ClientCreationStrategyFactory : IClientCreationStrategyFactory
    {
        private readonly ExistingClientStrategy _existingClientStrategy;
        private readonly NewUserStrategy _newUserStrategy;

        public ClientCreationStrategyFactory(
            ExistingClientStrategy existingClientStrategy,
            NewUserStrategy newUserStrategy)
        {
            _existingClientStrategy = existingClientStrategy;
            _newUserStrategy = newUserStrategy;
        }

        public IClientCreationStrategy GetStrategy(AppointmentCreateDto dto)
        {
            return dto.ClientId == null || dto.ClientId == 0
                ? _newUserStrategy
                : _existingClientStrategy;
        }
    }
}
