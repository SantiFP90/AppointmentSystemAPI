using AppointmentSystem.Application.Interfaces.Repositories;
using AppointmentSystem.Domain.Entities;
using AppointmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppointmentSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private IDbContextTransaction? _transaction;

        public IGenericRepository<User> Users { get; }
        public IGenericRepository<Role> Roles { get; }
        public IGenericRepository<WorkingDay> WorkingDays { get; }
        public IGenericRepository<TimeSlot> TimeSlots { get; }
        public IGenericRepository<Appointment> Appointments { get; }
        public IGenericRepository<NotificationTemplate> NotificationTemplates { get; }
        public IGenericRepository<NotificationLog> NotificationLogs { get; }

        public UnitOfWork(
            AppDbContext dbContext,
            IGenericRepository<User> userRepository,
            IGenericRepository<Role> roleRepository,
            IGenericRepository<WorkingDay> workingDayRepository,
            IGenericRepository<TimeSlot> timeSlotRepository,
            IGenericRepository<Appointment> appointmentRepository,
            IGenericRepository<NotificationTemplate> notificationTemplateRepository,
            IGenericRepository<NotificationLog> notificationLogRepository
        )
        {
            _dbContext = dbContext;
            Users = userRepository;
            Roles = roleRepository;
            WorkingDays = workingDayRepository;
            TimeSlots = timeSlotRepository;
            Appointments = appointmentRepository;
            NotificationTemplates = notificationTemplateRepository;
            NotificationLogs = notificationLogRepository;
        }

        // Guardar cambios
        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        // Manejo de transacciones
        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                throw new InvalidOperationException("Ya hay una transacción activa.");

            _transaction = await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No hay transacción activa.");

            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
                return;

            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        // Ejecutar operación con transacción automática
        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
        {
            var hasExistingTransaction = _transaction != null;

            if (!hasExistingTransaction)
                await BeginTransactionAsync();

            try
            {
                var result = await operation();

                if (!hasExistingTransaction)
                    await CommitTransactionAsync();

                return result;
            }
            catch
            {
                if (!hasExistingTransaction)
                    await RollbackTransactionAsync();

                throw;
            }
        }

        // Liberar recursos
        public async void Dispose()
        {
          await _dbContext.DisposeAsync();
        }
    }
}
