using AppointmentSystem.Application.Interfaces.Repositories;
using AppointmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentSystem.Infrastructure.Workers
{
    public class AppoimentDeleteTimeSlotWorker : BackgroundService
    {

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AppoimentDeleteTimeSlotWorker> _logger;
        private PeriodicTimer? _dailyTimer;

        public AppoimentDeleteTimeSlotWorker(IServiceScopeFactory scopeFactory, ILogger<AppoimentDeleteTimeSlotWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AppointmentNotificationWorker iniciado");
            _dailyTimer = new PeriodicTimer(TimeSpan.FromDays(1));

            //TestTimmer
            //_dailyTimer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            await Task.WhenAll(
                  CheckDailyAppointmentsAsync(stoppingToken)
            );
        }


        private async Task CheckDailyAppointmentsAsync(CancellationToken stoppingToken)
        {
            while (await _dailyTimer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var now = DateTime.Now;

                    var workingDays = await unitOfWork.WorkingDays
                             .GetAll(include: q => q
                                 .Include(w => w.TimeSlots)
                                     .ThenInclude(ts => ts.Appointments)) 
                             .Where(w => w.Date < now.Date)
                             .ToListAsync(stoppingToken);

                    foreach (var wd in workingDays)
                    {
                        var slotsToDelete = wd.TimeSlots
                            .Where(ts => !ts.Appointments.Any()) 
                            .ToList();

                        foreach (var ts in slotsToDelete)
                        {
                            await unitOfWork.TimeSlots.Delete(ts);
                        }
                    }
                    await unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Eliminados {Count} TimeSlots antiguos", workingDays.Sum(w => w.TimeSlots.Count));

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el zondeo diario de citas");

                }
            }
        }

    }
}
