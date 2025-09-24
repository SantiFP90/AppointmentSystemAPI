using AppointmentSystem.Application.Interfaces.Repositories;
using AppointmentSystem.Application.Interfaces.Services;
using AppointmentSystem.Domain.Entities;
using AppointmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppointmentSystem.Infrastructure.Workers
{
    public class AppointmentNotificationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AppointmentNotificationWorker> _logger;
        private PeriodicTimer? _dailyTimer;
        private PeriodicTimer? _frequentTimer;

        public AppointmentNotificationWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<AppointmentNotificationWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AppointmentNotificationWorker iniciado");

            _dailyTimer = new PeriodicTimer(TimeSpan.FromDays(1));
            _frequentTimer = new PeriodicTimer(TimeSpan.FromMinutes(5));

            await Task.WhenAll(
                CheckDailyAppointmentsAsync(stoppingToken),
                CheckNearAppointmentsAsync(stoppingToken)
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
                    var smtpService = scope.ServiceProvider.GetRequiredService<ISmtpService>();

                    var today = DateTime.Now;
                    var tomorrow = today.AddDays(1);

                    var appointments = await unitOfWork.Appointments
                        .GetAll(
                            filter: a =>
                                a.TimeSlot.WorkingDay.Date.Date == tomorrow &&
                                !a.NotificationLogs.Any(n => n.Type == NotificationType.ConfirmationEmail24),
                            include: a => a
                                .Include(x => x.NotificationLogs)
                                .Include(x => x.Client)
                                .Include(x => x.TimeSlot)
                                    .ThenInclude(ts => ts.WorkingDay)
                        )
                        .ToListAsync(stoppingToken);

                    _logger.LogInformation("Turnos para mañana: {Count}", appointments.Count);


                    foreach (var appointment in appointments)
                    {
                        await ProcessAppointmentNotification(
                            appointment,
                            NotificationType.ConfirmationEmail24,
                            "Recordatorio: Tienes una cita programada para mañana",
                            stoppingToken,
                            smtpService);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el zondeo diario de citas");
                }
            }
        }


        private async Task CheckNearAppointmentsAsync(CancellationToken stoppingToken)
        {
            while (await _frequentTimer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var smtpService = scope.ServiceProvider.GetRequiredService<ISmtpService>();

                    var now = DateTime.Now;
                    var oneHourLater = now.AddHours(1);

                    var appointments = await unitOfWork.Appointments
                        .GetAll(
                            include: a => a
                                .Include(x => x.TimeSlot)
                                    .ThenInclude(ts => ts.WorkingDay)
                                .Include(x => x.Client)
                                .Include(x => x.NotificationLogs)
                        )
                        .ToListAsync(stoppingToken);

                    var filtered = appointments
                        .Where(a =>
                        {
                            var appointmentDateTime = a.TimeSlot.WorkingDay.Date.Date + a.TimeSlot.StartTime;
                            return appointmentDateTime >= now
                                   && appointmentDateTime <= oneHourLater
                                   && !a.NotificationLogs.Any(n => n.Type == NotificationType.ConfirmationEmail);
                        })
                        .OrderBy(a => a.TimeSlot.WorkingDay.Date.Date + a.TimeSlot.StartTime)
                        .ToList();

                    _logger.LogInformation("Turnos próximos en la próxima hora: {Count}", filtered.Count);

                    foreach (var app in filtered)
                    {
                        var start = app.TimeSlot.WorkingDay.Date.Date + app.TimeSlot.StartTime;
                        _logger.LogInformation("Procesando notificación -> Id:{Id}, Cliente:{Client}, Inicio:{Start}",
                            app.Id, app.ClientName, start);

                        await ProcessAppointmentNotification(
                            app,
                            NotificationType.ConfirmationEmail,
                            "Recordatorio: Tu cita es en menos de una hora",
                            stoppingToken,
                            smtpService);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el zondeo de citas próximas");
                }
            }
        }


        private async Task ProcessAppointmentNotification(
            Appointment appointment,
            NotificationType type,
            string subject,
            CancellationToken stoppingToken,
            ISmtpService smtpService)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var notificationLog = new NotificationLog
                {
                    AppointmentId = appointment.Id,
                    Type = type,
                    Recipient = appointment.ClientEmail,
                    Subject = subject,
                    Body = $"Tienes una cita programada para el {appointment.TimeSlot.StartTime}",
                    CreatedAt = DateTime.UtcNow
                };

                var emailSent = await smtpService.SendEmailAsync(
                    appointment.ClientEmail,
                    subject,
                    notificationLog.Body);

                if (emailSent)
                {
                    notificationLog.IsSent = true;
                    notificationLog.SentAt = DateTime.UtcNow;

                    await unitOfWork.NotificationLogs.Create(notificationLog);
                    await unitOfWork.SaveChangesAsync();
                    _logger.LogInformation(
                        "Notificación enviada exitosamente para la cita {AppointmentId}",
                        appointment.Id);
                }
                else
                {
                    notificationLog.ErrorMessage = "Error al enviar el email";
                    await unitOfWork.NotificationLogs.Create(notificationLog);
                    await unitOfWork.SaveChangesAsync();
                    _logger.LogWarning(
                        "No se pudo enviar la notificación para la cita {AppointmentId}",
                        appointment.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al procesar notificación para la cita {AppointmentId}",
                    appointment.Id);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AppointmentNotificationWorker deteniéndose...");

            _dailyTimer?.Dispose();
            _frequentTimer?.Dispose();

            await base.StopAsync(stoppingToken);
        }
    }
}
