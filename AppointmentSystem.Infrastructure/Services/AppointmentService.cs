using AppointmentSystem.Application.DTOS.Appoiment;
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
                        include: q => q.Include(u => u.Client).Include(a => a.TimeSlot)
                    );
                    if (existing == null)
                        return null;

                    TimeSlot timeSlot = await _unitOfWork.TimeSlots.GetByItem(t => t.Id == existing.TimeSlotId);
                    
                    timeSlot.IsAvailable = true;

                    await _unitOfWork.TimeSlots.Edit(timeSlot);

                    await _unitOfWork.SaveChangesAsync();

                    existing.Status = dto.Status;

                    await _unitOfWork.Appointments.Edit(existing);
                    
                    await _unitOfWork.SaveChangesAsync();

                    var response = _mapper.Map<AppointmentDto>(existing);

                    return response;
                });
                if (result == null)
                    return ApiResponse<AppointmentDto>.Fail("Error al actualizar la cita");
                return ApiResponse<AppointmentDto>.Ok(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<AppointmentDto>.Fail($"Error al actualizar cita: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var existing = await _unitOfWork.Appointments.GetByItem(a => a.Id == id);

                    if (existing == null)
                        return false;

                    await _unitOfWork.Appointments.Delete(existing);
                    await _unitOfWork.SaveChangesAsync();

                    return true;
                });

                if (!result)
                    return ApiResponse<bool>.Fail("Ocurrió un error");

                return ApiResponse<bool>.Ok(true, "La cita se elimino correctamente");
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

        public async Task<ApiResponse<PaginatedResponse<AppointmentDto>>> GetAllPagedAsync(int page, int pageSize)
        {
            var paged = await _unitOfWork.Appointments.GetPagedAsync(
                page,
                pageSize,
                null,
                q => q.Include(a => a.Client).Include(a => a.TimeSlot),
                q => q.OrderBy(a => a.CreatedAt)
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

        private async Task<int> GetOrCreateClientIdAsync(AppointmentCreateDto dto)
        {
            var strategy = _clientFactory.GetStrategy(dto);
            return await strategy.GetOrCreateClientIdAsync(dto);
        }

        private async Task SendEmailAndLogAsync(AppointmentCreateDto dto, Appointment appointmentEntity)
        {
            // Cargar plantilla
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
                await _smtpService.SendEmailAsync(
                    dto.ClientEmail!,
                    "Confirmación de turno",
                    emailBody,
                    isHtml: true
                );
                emailSent = true;
            }
            catch (Exception ex)
            {
                emailSent = false;
                errorMessage = ex.Message;
            }

            // Guardar log
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
    }
}
