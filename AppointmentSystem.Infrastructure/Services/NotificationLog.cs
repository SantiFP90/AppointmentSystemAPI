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
    public class NotificationLogService : INotificationLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationLogService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<NotificationLogDto> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.NotificationLogs.GetByItem(
                x => x.Id == id,
                include: q => q.Include(l => l.Appointment)
            );

            if (entity == null)
                throw new KeyNotFoundException("Notification log not found");

            return _mapper.Map<NotificationLogDto>(entity);
        }

        public async Task<PaginatedResponse<NotificationLogDto>> GetAllAsync(int page, int pageSize, int? appointmentId = null, NotificationType? type = null, bool? isSent = null)
        {
            var paged = await _unitOfWork.NotificationLogs.GetPagedAsync(
                page,
                pageSize,
                x =>
                    (!appointmentId.HasValue || x.AppointmentId == appointmentId.Value) &&
                    (!type.HasValue || x.Type == type.Value) &&
                    (!isSent.HasValue || x.IsSent == isSent.Value),
                q => q
                    .Include(l => l.Appointment),
                q => q.OrderByDescending(l => l.CreatedAt)
            );

            return new PaginatedResponse<NotificationLogDto>
            {
                Items = _mapper.Map<List<NotificationLogDto>>(paged.Items),
                TotalItems = paged.TotalItems,
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages
            };
        }

        public async Task<NotificationLogDto> CreateAsync(NotificationLogDto logCreateDto)
        {
            var entity = _mapper.Map<NotificationLog>(logCreateDto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsSent = false;

            await _unitOfWork.NotificationLogs.Create(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<NotificationLogDto>(entity);
        }

        public async Task<NotificationLogDto> UpdateAsync(int id, NotificationLogDto logUpdateDto)
        {
            var entity = await _unitOfWork.NotificationLogs.GetByItem(x => x.Id == id);

            if (entity == null)
                throw new KeyNotFoundException("Notification log not found");

            _mapper.Map(logUpdateDto, entity);

            await _unitOfWork.NotificationLogs.Edit(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<NotificationLogDto>(entity);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.NotificationLogs.GetByItem(x => x.Id == id);

            if (entity == null)
                return ApiResponse<bool>.Fail("Notification log not found");

            await _unitOfWork.NotificationLogs.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true);
        }

        public async Task<IEnumerable<NotificationLogDto>> GetPendingNotificationsAsync()
        {
            var query = _unitOfWork.NotificationLogs.GetAll(
                x => !x.IsSent,
                q => q
                    .Include(l => l.Appointment)
            );

            var pending = await query
                .OrderBy(l => l.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<NotificationLogDto>>(pending);
        }
    }
}
