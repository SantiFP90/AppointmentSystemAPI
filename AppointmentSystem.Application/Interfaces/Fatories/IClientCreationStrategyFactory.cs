using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Application.DTOS.Appoiment;
using AppointmentSystem.Application.Interfaces.Strategies;

namespace AppointmentSystem.Application.Interfaces.Fatories
{
    public interface IClientCreationStrategyFactory
    {
        IClientCreationStrategy GetStrategy(AppointmentCreateDto dto);
    }
}
