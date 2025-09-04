using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Application.DTOS.Notification;
using AppointmentSystem.Application.DTOS.Response;
using AppointmentSystem.Application.Interfaces.Repositories;
using AppointmentSystem.Application.Interfaces.Services;
using AppointmentSystem.Domain.Entities;
using AppointmentSystem.Domain.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Infrastructure.Services
{
    public class NotificationTemplateService : INotificationTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationTemplateService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<NotificationTemplateDto> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.NotificationTemplates.GetByItem(
                x => x.Id == id,
                include: q => q.Include(t => t.NotificationLogs)
            );

            if (entity == null)
                throw new KeyNotFoundException("Notification template not found");

            return _mapper.Map<NotificationTemplateDto>(entity);
        }

        public async Task<PaginatedResponse<NotificationTemplateDto>> GetAllAsync(int page, int pageSize, NotificationType? type = null, bool? isActive = null)
        {
            var paged = await _unitOfWork.NotificationTemplates.GetPagedAsync(
                page,
                pageSize,
                x =>
                    (!type.HasValue || x.Type == type.Value) &&
                    (!isActive.HasValue || x.IsActive == isActive.Value),
                null,
                q => q.OrderBy(t => t.Name)
            );

            return new PaginatedResponse<NotificationTemplateDto>
            {
                Items = _mapper.Map<List<NotificationTemplateDto>>(paged.Items),
                TotalItems = paged.TotalItems,
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages
            };
        }

        public async Task<NotificationTemplateDto> CreateAsync(NotificationTemplateDto templateCreateDto)
        {
            var entity = _mapper.Map<NotificationTemplate>(templateCreateDto);

            // aseguramos valores por defecto
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;

            await _unitOfWork.NotificationTemplates.Create(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<NotificationTemplateDto>(entity);
        }

        public async Task<NotificationTemplateDto> UpdateAsync(int id, NotificationTemplateDto templateUpdateDto)
        {
            var entity = await _unitOfWork.NotificationTemplates.GetByItem(x => x.Id == id);

            if (entity == null)
                throw new KeyNotFoundException("Notification template not found");

            // mapeamos las propiedades actualizables
            _mapper.Map(templateUpdateDto, entity);

            await _unitOfWork.NotificationTemplates.Edit(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<NotificationTemplateDto>(entity);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.NotificationTemplates.GetByItem(x => x.Id == id);

            if (entity == null)
                return ApiResponse<bool>.Fail("Notification template not found");

            await _unitOfWork.NotificationTemplates.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true);
        }
    }
}
