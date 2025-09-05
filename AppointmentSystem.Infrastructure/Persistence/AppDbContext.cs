using Microsoft.EntityFrameworkCore;
using AppointmentSystem.Domain.Entities;

namespace AppointmentSystem.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<WorkingDay> WorkingDays => Set<WorkingDay>();
        public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        //Comands of migrations
        //dotnet ef migrations add nameMigration --project../AppointmentSystem.Infrastructure/AppointmentSystem.Infrastructure.csproj
        //dotnet ef database update --project../AppointmentSystem.Infrastructure/AppointmentSystem.Infrastructure.csproj
    }
}