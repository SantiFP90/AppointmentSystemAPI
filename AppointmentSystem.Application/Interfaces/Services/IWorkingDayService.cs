using AppointmentSystem.Application.DTOS.Response;
using AppointmentSystem.Application.DTOS.WorkingDay;

namespace AppointmentSystem.Application.Interfaces.Services
{
    public interface IWorkingDayService
    {
        Task<ApiResponse<WorkingDayDto>> CreateAsync(WorkingDayCreateDto dto);
        Task<ApiResponse<WorkingDayDto>> UpdateAsync(int id, WorkingDayDto dto); 
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<WorkingDayDto>> GetByIdAsync(int id);
        Task<ApiResponse<PaginatedResponse<WorkingDayDto>>> GetAllPagedAsync(int page, int pageSize);
    }

}
