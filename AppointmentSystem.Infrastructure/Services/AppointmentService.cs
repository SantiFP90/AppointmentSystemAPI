using AppointmentSystem.Application.DTOS.Appoiment;
using AppointmentSystem.Application.DTOS.Calendar;
using AppointmentSystem.Application.DTOS.Notification;
using AppointmentSystem.Application.DTOS.Response;
using AppointmentSystem.Application.DTOS.TimeSlot;
using AppointmentSystem.Application.Interfaces.Fatories;
using AppointmentSystem.Application.Interfaces.Repositories;
using AppointmentSystem.Application.Interfaces.Services;
using AppointmentSystem.Domain.Entities;
using AppointmentSystem.Domain.Enums;
using AppointmentSystem.Infrastructure.Helper;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net.Mail;

namespace AppointmentSystem.Infrastructure.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClientCreationStrategyFactory _clientFactory;
        private readonly INotificationLogService _notificationLogService;
        private readonly ISmtpService _smtpService;

        public AppointmentService(IUnitOfWork unitOfWork, IMapper mapper, IClientCreationStrategyFactory clientFactory, INotificationLogService notificationLogService, ISmtpService smtpService)
        {
            _unitOfWork = unitOfWork;
            _clientFactory = clientFactory;
            _mapper = mapper;
            _notificationLogService = notificationLogService;
            _smtpService = smtpService;
        }

        public async Task<ApiResponse<AppointmentDto>> CreateAsync(AppointmentCreateDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                TimeSlot timeSlot = await _unitOfWork.TimeSlots.GetByItem(t => t.Id == dto.TimeSlotId);

                if (timeSlot == null)
                {
                    return ApiResponse<AppointmentDto>.Fail("El turno seleccionado no existe.");
                }

                if (!timeSlot.IsAvailable)
                {
                    return ApiResponse<AppointmentDto>.Fail("El turno seleccionado ya está reservado.");
                }

                dto.ClientId = await GetOrCreateClientIdAsync(dto);

                var appointmentEntity = _mapper.Map<Appointment>(dto);

                await _unitOfWork.Appointments.Create(appointmentEntity);

                timeSlot.IsAvailable = false;

                await _unitOfWork.TimeSlots.Edit(timeSlot);

                await _unitOfWork.SaveChangesAsync();

                var appointmentWithRelations = await _unitOfWork.Appointments.GetByItem(
                        a => a.Id == appointmentEntity.Id,
                        include: q => q.Include(a => a.TimeSlot)
                                       .ThenInclude(ts => ts.WorkingDay)
                );

                await SendEmailAndLogAsync(dto, appointmentWithRelations!);

                await _unitOfWork.CommitTransactionAsync();

                return ApiResponse<AppointmentDto>.Ok(_mapper.Map<AppointmentDto>(appointmentEntity));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<AppointmentDto>.Fail($"Error al crear cita: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AppointmentDto>> UpdateAsync(int id, AppointmentUpdateDto dto)
        {
            try
            {
                var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var existing = await _unitOfWork.Appointments.GetByItem(
                        a => a.Id == id,
                        include: q => q.Include(a => a.TimeSlot)
                                       .ThenInclude(ts => ts.WorkingDay)
                    );

                    if (existing == null)
                        return null;

                    var validation = await ValidateAndSwapTimeSlotAsync(existing, dto);
                    if (!validation.Success)
                        throw new InvalidOperationException(validation.Message);

                    existing.Status = dto.Status;
                    existing.PaymentStatus = dto.PaymentStatus;

                    await _unitOfWork.Appointments.Edit(existing);
                    await _unitOfWork.SaveChangesAsync();

                    return _mapper.Map<AppointmentDto>(existing);
                });

                return result == null
                    ? ApiResponse<AppointmentDto>.Fail("No se encontró la cita.")
                    : ApiResponse<AppointmentDto>.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<AppointmentDto>.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return ApiResponse<AppointmentDto>.Fail($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var existing = await _unitOfWork.Appointments.GetByItem(
                        a => a.Id == id,
                        include: q => q.Include(a => a.TimeSlot)
                    );

                    if (existing == null)
                        return ApiResponse<bool>.Fail("La cita no existe.");

                    if (existing.TimeSlot != null)
                    {
                        existing.TimeSlot.IsAvailable = true;
                        await _unitOfWork.TimeSlots.Edit(existing.TimeSlot);
                    }

                    await _unitOfWork.Appointments.Delete(existing);

                    await _unitOfWork.SaveChangesAsync();

                    var registerLog = await _notificationLogService.CreateAsync(new NotificationLogDto
                    {
                        AppointmentId = existing.Id,
                        Type = NotificationType.Email,
                        Recipient = existing.Client?.Email ?? "N/A",
                        Subject = "Cita cancelada",
                        Body = $"La cita para el {existing.TimeSlot?.WorkingDay.Date:dd/MM/yyyy} a las {existing.TimeSlot?.StartTime:hh\\:mm} ha sido cancelada.",
                        IsSent = false,
                        SentAt = DateTime.UtcNow
                    });

                    if (registerLog.IsSent == false)
                    {
                        await _smtpService.SendEmailAsync(
                            existing.Client?.Email ?? "N/A",
                            "Cita cancelada",
                            $"La cita para el {existing.TimeSlot?.WorkingDay.Date:dd/MM/yyyy} a las {existing.TimeSlot?.StartTime:hh\\:mm} ha sido cancelada.",
                            isHtml: false
                        );
                        registerLog.IsSent = true;
                        registerLog.SentAt = DateTime.UtcNow;
                        await _notificationLogService.UpdateAsync(registerLog.Id, registerLog);
                    }

                    return ApiResponse<bool>.Ok(true, "La cita fue eliminada y el turno liberado.");
                });

                return result;
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Error al eliminar la cita: {ex.Message}");
            }
        }


        public async Task<ApiResponse<AppointmentDto>> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.Appointments.GetByItem(
                a => a.Id == id,
                include: q => q.Include(u => u.Client).Include(a => a.TimeSlot)
            );

            return entity == null
                ? ApiResponse<AppointmentDto>.Fail("Appointment not found")
                : ApiResponse<AppointmentDto>.Ok(_mapper.Map<AppointmentDto>(entity));
        }

        public async Task<ApiResponse<PaginatedResponse<AppointmentDto>>> GetAllPagedAsync(int page, int pageSize, string name)
        {
            Expression<Func<Appointment, bool>>? filter = null;

            if (!string.IsNullOrWhiteSpace(name))
            {
                filter = a => a.Client.FullName.Contains(name);
            }

            var paged = await _unitOfWork.Appointments.GetPagedAsync(
                page,
                pageSize,
                filter,
                q => q.Include(a => a.Client)
                      .Include(a => a.TimeSlot)
                      .ThenInclude(ts => ts.WorkingDay),
                q => q.OrderByDescending(a => a.TimeSlot.WorkingDay.Date)
            );

            var mapped = new PaginatedResponse<AppointmentDto>
            {
                Items = _mapper.Map<List<AppointmentDto>>(paged.Items),
                TotalItems = paged.TotalItems,
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages
            };

            return ApiResponse<PaginatedResponse<AppointmentDto>>.Ok(mapped);
        }

        public async Task<ApiResponse<List<AppointmentDto>>> GetByUserIdAsync(int userId)
        {
            var query = _unitOfWork.Appointments.GetAll(
                a => a.ClientId == userId,
                q => q.Include(a => a.Client).Include(a => a.TimeSlot)
            );

            var list = await query.OrderBy(a => a.CreatedAt).ToListAsync();

            return ApiResponse<List<AppointmentDto>>.Ok(_mapper.Map<List<AppointmentDto>>(list));
        }

        public async Task<ApiResponse<List<AppointmentDto>>> GetByDateAsync(DateTime date)
        {
            var query = _unitOfWork.Appointments.GetAll(
               a => a.CreatedAt == date,
               q => q.Include(a => a.Client).Include(a => a.TimeSlot)
           );

            var list = await query.OrderBy(a => a.TimeSlot.StartTime).ToListAsync();

            return ApiResponse<List<AppointmentDto>>.Ok(_mapper.Map<List<AppointmentDto>>(list));
        }

        public async Task<ApiResponse<List<AppoimentMonthDto>>> GetAppoimentByMonth(int month)
        {
            var appointments = await _unitOfWork.Appointments.GetAll(
                a => a.TimeSlot.WorkingDay.Date.Month == month,
                q => q.Include(a => a.Client)
                      .Include(a => a.TimeSlot)
                      .ThenInclude(ts => ts.WorkingDay)
            ).ToListAsync();

            var result = _mapper.Map<List<AppoimentMonthDto>>(appointments);

            return ApiResponse<List<AppoimentMonthDto>>.Ok(result);
        }

        public async Task<ApiResponse<List<CalendarDayDto>>> GetCalendarByMonthAsync(int month, int year)
        {
            var workingDays = await _unitOfWork.WorkingDays.GetAll(
                wd => wd.Date.Month == month && wd.Date.Year == year,
                q => q.Include(wd => wd.TimeSlots)
                      .ThenInclude(ts => ts.Appointments)
                      .ThenInclude(a => a.Client)
            ).ToListAsync();

            var result = workingDays.Select(wd => new CalendarDayDto
            {
                WorkingDayId = wd.Id,
                Date = wd.Date,
                TimeSlots = wd.TimeSlots.Select(ts => new CalendarTimeSlotDto
                {
                    StartTime = ts.StartTime,
                    EndTime = ts.EndTime,
                    IsAvailable = !ts.Appointments.Any(),
                    Appointment = ts.Appointments.Select(a => new AppointmentDto
                    {
                        ClientId = a.Client.Id,
                        ClientName = a.Client.FullName,
                        ClientEmail = a.Client.Email,
                        Status = a.Status
                    }).FirstOrDefault()
                }).ToList()
            }).ToList();
            return ApiResponse<List<CalendarDayDto>>.Ok(result);
        }

        public async Task<ApiResponse<AppoimentCounterDto>> GetCounters()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            var endOfWeek = startOfWeek.AddDays(6);

            var counters = new AppoimentCounterDto
            {
                AvailableAppointments = await _unitOfWork.Appointments
                    .GetAll(a => a.TimeSlot.IsAvailable == false)
                    .CountAsync(),
                PedingAppointmentsToday = await _unitOfWork.Appointments
                    .GetAll(a => a.TimeSlot.WorkingDay.Date >= today &&
                                 a.TimeSlot.WorkingDay.Date < tomorrow)
                    .CountAsync(),
                PendingAppoimentsThisWeek = await _unitOfWork.Appointments
                    .GetAll(a => a.TimeSlot.WorkingDay.Date >= today &&
                                 a.TimeSlot.WorkingDay.Date <= endOfWeek)
                    .CountAsync()
            };
            return ApiResponse<AppoimentCounterDto>.Ok(counters);
        }


        #region Functions
        private async Task<int> GetOrCreateClientIdAsync(AppointmentCreateDto dto)
        {
            var strategy = _clientFactory.GetStrategy(dto);
            return await strategy.GetOrCreateClientIdAsync(dto);
        }

        private async Task SendEmailAndLogAsync(AppointmentCreateDto dto, Appointment appointmentEntity)
        {
            var emailBody = EmailTemplateHelper.LoadTemplate(
                "AppointmentConfirmation.html",
                new Dictionary<string, string>
                {
            { "ClientName", dto.ClientName },
            { "Date", appointmentEntity.TimeSlot.WorkingDay.Date.ToString("dd/MM/yyyy") },
            { "Time", appointmentEntity.TimeSlot.StartTime.ToString(@"hh\:mm") }
                }
            );

            bool emailSent = false;
            string? errorMessage = null;

            try
            {
                var calendarAttachment = GenerateCalendarInvite(dto, appointmentEntity);

                await _smtpService.SendEmailAsync(
                    dto.ClientEmail!,
                    "Confirmación de turno",
                    emailBody,
                    isHtml: true,
                    attachments: new List<Attachment> { calendarAttachment }
                );

                emailSent = true;
            }
            catch (Exception ex)
            {
                emailSent = false;
                errorMessage = ex.Message;
            }

            var logDto = new NotificationLogDto
            {
                AppointmentId = appointmentEntity.Id,
                Type = NotificationType.Email,
                Recipient = dto.ClientEmail!,
                Subject = "Confirmación de turno",
                Body = emailBody,
                IsSent = emailSent,
                SentAt = emailSent ? DateTime.UtcNow : null,
                ErrorMessage = errorMessage
            };

            await _notificationLogService.CreateAsync(logDto);
        }


        private Attachment GenerateCalendarInvite(AppointmentCreateDto dto, Appointment appointment)
        {
            // Convertimos fecha y hora a UTC
            var startUtc = appointment.TimeSlot.WorkingDay.Date
                            .Add(appointment.TimeSlot.StartTime)
                            .ToUniversalTime();
            var endUtc = appointment.TimeSlot.WorkingDay.Date
                            .Add(appointment.TimeSlot.EndTime)
                            .ToUniversalTime();

            string dtStamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");
            string dtStart = startUtc.ToString("yyyyMMdd'T'HHmmss'Z'");
            string dtEnd = endUtc.ToString("yyyyMMdd'T'HHmmss'Z'");
            string uid = Guid.NewGuid().ToString();

            string organizerName = "Mi Empresa";
            string organizerEmail = "servicesmailatr@gmail.com";

            string icsContent = $@"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Mi Empresa//Appointment Scheduler//ES
CALSCALE:GREGORIAN
METHOD:REQUEST
BEGIN:VEVENT
UID:{uid}
DTSTAMP:{dtStamp}
DTSTART:{dtStart}
DTEND:{dtEnd}
SUMMARY:Turno con {dto.ClientName}
DESCRIPTION:Turno agendado
LOCATION:Tu ubicación
ORGANIZER;CN={organizerName}:MAILTO:{organizerEmail}
ATTENDEE;CN={dto.ClientName};RSVP=TRUE:MAILTO:{dto.ClientEmail}
STATUS:CONFIRMED
BEGIN:VALARM
TRIGGER:-PT15M
ACTION:DISPLAY
DESCRIPTION:Recordatorio
END:VALARM
END:VEVENT
END:VCALENDAR";

            var bytes = System.Text.Encoding.UTF8.GetBytes(icsContent);
            var stream = new MemoryStream(bytes);
            var attachment = new Attachment(stream, "appointment.ics", "text/calendar; method=REQUEST; charset=UTF-8");
            return attachment;
        }




        private async Task<ApiResponse<bool>> ValidateAndSwapTimeSlotAsync(Appointment existing, AppointmentUpdateDto dto)
        {
            if (!dto.TimeSlotId.HasValue || dto.TimeSlotId.Value == existing.TimeSlotId)
                return ApiResponse<bool>.Ok(true);

            var newTimeSlot = await _unitOfWork.TimeSlots.GetByItem(t => t.Id == dto.TimeSlotId.Value);
            if (newTimeSlot == null || !newTimeSlot.IsAvailable)
                return ApiResponse<bool>.Fail("El nuevo turno seleccionado no está disponible.");

            existing.TimeSlot.IsAvailable = true;
            await _unitOfWork.TimeSlots.Edit(existing.TimeSlot);

            newTimeSlot.IsAvailable = false;
            await _unitOfWork.TimeSlots.Edit(newTimeSlot);

            existing.TimeSlotId = newTimeSlot.Id;
            return ApiResponse<bool>.Ok(true);
        }
        #endregion
    }
}
