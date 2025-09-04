using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Application.DTOS.Auth;
using AppointmentSystem.Application.DTOS.Response;
using AppointmentSystem.Application.DTOS.User;


namespace AppointmentSystem.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginUser);
        Task<ApiResponse<UserDto>> RegisterAsync(RegisterUserDto registerUser);
        Task<ApiResponse<UserDto>> GetByIdAsync(int id);
        Task<ApiResponse<bool>> DeleteById(int id);
        Task<ApiResponse<UserDto>> UpdateById(int id, UserDto updateUser);
        Task<ApiResponse<UserDto>> UpdateByIdForClient(int id, RegisterUserDto updateUser);

    }
}
