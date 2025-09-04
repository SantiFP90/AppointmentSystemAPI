using AppointmentSystem.Application.DTOS.Response;
using AppointmentSystem.Application.DTOS.TimeSlot;
using AppointmentSystem.Application.DTOS.WorkingDay;
using AppointmentSystem.Application.Interfaces.Repositories;
using AppointmentSystem.Application.Interfaces.Services;
using AppointmentSystem.Domain.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Infrastructure.Services
{
    public class WorkingDayService : IWorkingDayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WorkingDayService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<WorkingDayDto>> CreateAsync(WorkingDayCreateDto dto)
        {
            try
            {
                var created = await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var workingDay = _mapper.Map<WorkingDay>(dto);
                    workingDay.IsActive = true;

                    await _unitOfWork.WorkingDays.Create(workingDay);
                    await _unitOfWork.SaveChangesAsync();


                    var workingDayCreated = await _unitOfWork.WorkingDays.GetByItem(wd => wd.Id == workingDay.Id);

                    int idWorkingDay = workingDayCreated!.Id;

                    int totalTimes = GetTimeSlotCount(
                        workingDayCreated.StartTime,
                        workingDayCreated.EndTime,
                        workingDayCreated.SlotDurationMinutes
                    );

                    var currentStart = workingDayCreated.StartTime;
                    var slotDuration = TimeSpan.FromMinutes(workingDayCreated.SlotDurationMinutes);

                    for (int i = 0; i < totalTimes; i++)
                    {
                        var timeSlotEntity = new TimeSlot
                        {
                            WorkingDayId = idWorkingDay,
                            StartTime = currentStart,
                            EndTime = currentStart.Add(slotDuration),
                            IsAvailable = true
                        };

                        await _unitOfWork.TimeSlots.Create(timeSlotEntity);

                        currentStart = currentStart.Add(slotDuration); 
                    }

                    await _unitOfWork.SaveChangesAsync();

                    return workingDay;
                });

                return ApiResponse<WorkingDayDto>.Ok(_mapper.Map<WorkingDayDto>(created), "Working day created successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<WorkingDayDto>.Fail($"Error al crear WorkingDay: {ex.Message}");
            }
        }

        public async Task<ApiResponse<WorkingDayDto>> UpdateAsync(int id, WorkingDayDto dto)
        {
            try
            {
                var updated = await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var existing = await _unitOfWork.WorkingDays.GetByItem(wd => wd.Id == id);
                    if (existing is null) return null;

                    existing.Date = dto.Date;
                    existing.StartTime = dto.StartTime;
                    existing.EndTime = dto.EndTime;
                    existing.SlotDurationMinutes = dto.SlotDurationMinutes;
                    existing.IsActive = dto.IsActive;

                    await _unitOfWork.WorkingDays.Edit(existing);
                    await _unitOfWork.SaveChangesAsync();

                    return existing;
                });

                if (updated is null)
                    return ApiResponse<WorkingDayDto>.Fail("Working day not found.");

                return ApiResponse<WorkingDayDto>.Ok(_mapper.Map<WorkingDayDto>(updated), "Working day updated successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<WorkingDayDto>.Fail($"Error al actualizar WorkingDay: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var existing = await _unitOfWork.WorkingDays.GetByItem(wd => wd.Id == id);
                    if (existing is null) return false;

                    await _unitOfWork.WorkingDays.Delete(existing);
                    await _unitOfWork.SaveChangesAsync();

                    return true;
                });

                if (!result)
                    return ApiResponse<bool>.Fail("Working day not found.");

                return ApiResponse<bool>.Ok(true, "Working day deleted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Error al eliminar WorkingDay: {ex.Message}");
            }
        }

        // Métodos de solo lectura → no requieren transacción
        public async Task<ApiResponse<WorkingDayDto>> GetByIdAsync(int id)
        {
            var workingDay = await _unitOfWork.WorkingDays.GetByItem(wd => wd.Id == id);
            if (workingDay is null)
                return ApiResponse<WorkingDayDto>.Fail("Working day not found.");

            var dto = _mapper.Map<WorkingDayDto>(workingDay);
            return ApiResponse<WorkingDayDto>.Ok(dto);
        }

        public async Task<ApiResponse<PaginatedResponse<WorkingDayDto>>> GetAllPagedAsync(int page, int pageSize)
        {
            var query =  _unitOfWork.WorkingDays.GetAll();

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(wd => wd.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginated = new PaginatedResponse<WorkingDayDto>
            {
                Items = _mapper.Map<List<WorkingDayDto>>(items),
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            return ApiResponse<PaginatedResponse<WorkingDayDto>>.Ok(paginated);
        }

        public int GetTimeSlotCount(TimeSpan start, TimeSpan end, int intervalMinutes)
        {
            if (intervalMinutes <= 0)
                throw new ArgumentException("El intervalo debe ser mayor a 0");

            var totalMinutes = (end - start).TotalMinutes;

            if (totalMinutes <= 0)
                return 0;

            return (int)(totalMinutes / intervalMinutes);
        }

    }
}


