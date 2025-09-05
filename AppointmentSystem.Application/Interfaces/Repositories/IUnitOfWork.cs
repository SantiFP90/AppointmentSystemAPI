using AppointmentSystem.Domain.Entities;

namespace AppointmentSystem.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }
        IGenericRepository<Role> Roles { get; }
        IGenericRepository<WorkingDay> WorkingDays { get; }
        IGenericRepository<TimeSlot> TimeSlots { get; }
        IGenericRepository<Appointment> Appointments { get; }
        IGenericRepository<NotificationLog> NotificationLogs { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
    }
}
