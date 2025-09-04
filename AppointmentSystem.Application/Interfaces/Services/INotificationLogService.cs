using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Application.DTOS.Notification;
using AppointmentSystem.Application.DTOS.Response;
using AppointmentSystem.Domain.Enums;

namespace AppointmentSystem.Application.Interfaces.Services
{
    public interface INotificationLogService
    {
        Task<NotificationLogDto> GetByIdAsync(int id);
        Task<PaginatedResponse<NotificationLogDto>> GetAllAsync(int page, int pageSize, int? appointmentId = null, NotificationType? type = null, bool? isSent = null);
        Task<NotificationLogDto> CreateAsync(NotificationLogDto logCreateDto);
        Task<NotificationLogDto> UpdateAsync(int id, NotificationLogDto logUpdateDto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<IEnumerable<NotificationLogDto>> GetPendingNotificationsAsync();
    }
}
