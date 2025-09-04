using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Application.DTOS.Response;
using AppointmentSystem.Application.DTOS.TimeSlot;
using AppointmentSystem.Application.Interfaces.Repositories;
using AppointmentSystem.Application.Interfaces.Services;
using AppointmentSystem.Domain.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Infrastructure.Services
{
    public class TimeSlotService : ITimeSlotService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TimeSlotService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<TimeSlotDto>> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.TimeSlots.GetByItem(
                ts => ts.Id == id,
                include: q => q.Include(t => t.WorkingDay)
            );

            if (entity == null)
                return ApiResponse<TimeSlotDto>.Fail("TimeSlot no encontrado");

            return ApiResponse<TimeSlotDto>.Ok(_mapper.Map<TimeSlotDto>(entity));
        }

        public async Task<ApiResponse<PaginatedResponse<TimeSlotDto>>> GetAllAsync(int page, int pageSize, int? workingDayId = null, bool? isAvailable = null)
        {
            Expression<Func<TimeSlot, bool>>? filter = null;

            if (workingDayId.HasValue || isAvailable.HasValue)
            {
                filter = ts =>
                    (!workingDayId.HasValue || ts.WorkingDayId == workingDayId.Value) &&
                    (!isAvailable.HasValue || ts.IsAvailable == isAvailable.Value);
            }

            var paged = await _unitOfWork.TimeSlots.GetPagedAsync(
                page,
                pageSize,
                filter,
                q => q.Include(t => t.WorkingDay),
                q => q.OrderBy(ts => ts.StartTime)
            );

            var result = new PaginatedResponse<TimeSlotDto>
            {
                Items = _mapper.Map<List<TimeSlotDto>>(paged.Items),
                TotalItems = paged.TotalItems,
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages
            };

            return ApiResponse<PaginatedResponse<TimeSlotDto>>.Ok(result);
        }

        public async Task<ApiResponse<TimeSlotDto>> CreateAsync(TimeSlotDto timeSlotDto)
        {
            try
            {
                var created = await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var entity = _mapper.Map<TimeSlot>(timeSlotDto);

                    await _unitOfWork.TimeSlots.Create(entity);
                    await _unitOfWork.SaveChangesAsync();

                    return entity;
                });

                return ApiResponse<TimeSlotDto>.Ok(_mapper.Map<TimeSlotDto>(created), "TimeSlot creado con éxito");
            }
            catch (Exception ex)
            {
                return ApiResponse<TimeSlotDto>.Fail($"Error al crear TimeSlot: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TimeSlotDto>> UpdateAsync(int id, TimeSlotDto timeSlotDto)
        {
            try
            {
                var updated = await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var existing = await _unitOfWork.TimeSlots.GetByItem(ts => ts.Id == id);
                    if (existing == null)
                        return null;

                    existing.StartTime = timeSlotDto.StartTime;
                    existing.EndTime = timeSlotDto.EndTime;
                    existing.IsAvailable = timeSlotDto.IsAvailable;
                    existing.WorkingDayId = timeSlotDto.WorkingDayId;

                    await _unitOfWork.TimeSlots.Edit(existing);
                    await _unitOfWork.SaveChangesAsync();

                    return existing;
                });

                if (updated == null)
                    return ApiResponse<TimeSlotDto>.Fail("TimeSlot no encontrado");

                return ApiResponse<TimeSlotDto>.Ok(_mapper.Map<TimeSlotDto>(updated), "TimeSlot actualizado con éxito");
            }
            catch (Exception ex)
            {
                return ApiResponse<TimeSlotDto>.Fail($"Error al actualizar TimeSlot: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var existing = await _unitOfWork.TimeSlots.GetByItem(ts => ts.Id == id);
                    if (existing == null)
                        return false;

                    await _unitOfWork.TimeSlots.Delete(existing);
                    await _unitOfWork.SaveChangesAsync();
                    return true;
                });

                if (!result)
                    return ApiResponse<bool>.Fail("TimeSlot no encontrado");

                return ApiResponse<bool>.Ok(true, "TimeSlot eliminado con éxito");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Error al eliminar TimeSlot: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<TimeSlotDto>>> GetAvailableByDayAsync(int workingDayId)
        {
            var query = _unitOfWork.TimeSlots.GetAll(
                ts => ts.WorkingDayId == workingDayId && ts.IsAvailable,
                q => q.Include(t => t.WorkingDay)
            );

            var list = await query.OrderBy(ts => ts.StartTime).ToListAsync();
            var dtos = _mapper.Map<IEnumerable<TimeSlotDto>>(list);

            return ApiResponse<IEnumerable<TimeSlotDto>>.Ok(dtos, "Consulta realizada con éxito");
        }
    }

}
