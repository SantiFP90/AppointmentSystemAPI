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
    public interface INotificationTemplateService
    {
        Task<NotificationTemplateDto> GetByIdAsync(int id);
        Task<PaginatedResponse<NotificationTemplateDto>> GetAllAsync(int page, int pageSize, NotificationType? type = null, bool? isActive = null);
        Task<NotificationTemplateDto> CreateAsync(NotificationTemplateDto templateCreateDto);
        Task<NotificationTemplateDto> UpdateAsync(int id, NotificationTemplateDto templateUpdateDto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
