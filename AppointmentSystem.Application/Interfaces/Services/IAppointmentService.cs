using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Application.DTOS.Appoiment;
using AppointmentSystem.Application.DTOS.Calendar;
using AppointmentSystem.Application.DTOS.Response;

namespace AppointmentSystem.Application.Interfaces.Services
{
    public interface IAppointmentService
    {
        Task<ApiResponse<AppointmentDto>> CreateAsync(AppointmentCreateDto dto);
        Task<ApiResponse<AppointmentDto>> UpdateAsync(int id, AppointmentUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<AppointmentDto>> GetByIdAsync(int id);
        Task<ApiResponse<PaginatedResponse<AppointmentDto>>> GetAllPagedAsync(int page, int pageSize, string name);
        Task<ApiResponse<List<AppointmentDto>>> GetByUserIdAsync(int userId);
        Task<ApiResponse<List<AppointmentDto>>> GetByDateAsync(DateTime date);
        Task<ApiResponse<List<AppoimentMonthDto>>> GetAppoimentByMonth(int month);
        Task<ApiResponse<List<CalendarDayDto>>> GetCalendarByMonthAsync(int month, int year);
        Task<ApiResponse<AppoimentCounterDto>> GetCounters();
    }

}
