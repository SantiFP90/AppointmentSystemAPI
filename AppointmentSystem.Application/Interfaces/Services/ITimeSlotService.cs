using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Application.DTOS.Response;
using AppointmentSystem.Application.DTOS.TimeSlot;

namespace AppointmentSystem.Application.Interfaces.Services
{
    public interface ITimeSlotService
    {
        Task<ApiResponse<TimeSlotDto>> GetByIdAsync(int id);
        Task<ApiResponse<PaginatedResponse<TimeSlotDto>>> GetAllAsync(int page, int pageSize, int? workingDayId = null, bool? isAvailable = null);
        Task<ApiResponse<TimeSlotDto>> CreateAsync(TimeSlotDto timeSlotCreateDto);
        Task<ApiResponse<TimeSlotDto>> UpdateAsync(int id, TimeSlotDto timeSlotUpdateDto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<IEnumerable<TimeSlotDto>>> GetAvailableByDayAsync(int workingDayId);
    }

}
