using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Application.DTOS.Response;
using AppointmentSystem.Application.DTOS.Role;

namespace AppointmentSystem.Application.Interfaces.Services
{
    public interface IRoleService
    {
        Task<ApiResponse<RoleDto>> CreateAsync(RoleDto dto); 
        Task<ApiResponse<RoleDto>> UpdateAsync(int id, RoleDto dto); 
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<RoleDto>> GetByIdAsync(int id);
        Task<ApiResponse<PaginatedResponse<RoleDto>>> GetAllPagedAsync(int page, int pageSize);
    }

}
