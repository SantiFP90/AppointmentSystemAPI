using AppointmentSystem.Application.DTOS.Appoiment;

namespace AppointmentSystem.Application.Interfaces.Strategies
{
    public interface IClientCreationStrategy
    {
        Task<int> GetOrCreateClientIdAsync(AppointmentCreateDto dto);
    }
}
