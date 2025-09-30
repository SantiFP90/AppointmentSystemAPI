using AppointmentSystem.Application.DTOS.Auth;
using AppointmentSystem.Application.DTOS.Response;
using AppointmentSystem.Application.DTOS.User;
using AppointmentSystem.Application.Interfaces.Repositories;
using AppointmentSystem.Application.Interfaces.Services;
using AppointmentSystem.Domain.Entities;
using AppointmentSystem.Infrastructure.Security;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSecurityService _jwtSecurityService;

        public AuthService(IMapper mapper, IUnitOfWork unitOfWork, JwtSecurityService jwtSecurityService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _jwtSecurityService = jwtSecurityService;
        }

        public async Task<ApiResponse<bool>> DeleteById(int id)
        {
            try
            {
                var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var user = await _unitOfWork.Users.GetByItem(u => u.Id == id);
                    if (user == null)
                        return false;

                    await _unitOfWork.Users.Delete(user);
                    await _unitOfWork.SaveChangesAsync();

                    return true;
                });

                if (!result)
                    return ApiResponse<bool>.Fail("Usuario no encontrado.");

                return ApiResponse<bool>.Ok(true, "Usuario eliminado correctamente.");
            }
            catch
            {
                return ApiResponse<bool>.Fail("Ocurrió un error al eliminar el usuario.");
            }
        }

        public async Task<ApiResponse<UserDto>> GetByIdAsync(int id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByItem(
                    u => u.Id == id,
                    include: q => q.Include(u => u.Role));

                if (user == null)
                    return ApiResponse<UserDto>.Fail("No existe el usuario");

                var userResponse = _mapper.Map<UserDto>(user);
                return ApiResponse<UserDto>.Ok(userResponse, "Usuario encontrado con éxito.");
            }
            catch
            {
                return ApiResponse<UserDto>.Fail("Ocurrió un error al obtener el usuario.");
            }
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginDto)
        {
            try
            {
                string normalizedEmail = loginDto.Email.Trim().ToLowerInvariant();

                var user = await _unitOfWork.Users.GetByItem(
                    u => u.Email == normalizedEmail,
                    include: q => q.Include(u => u.Role)
                );

                if(user == null)
                    return ApiResponse<LoginResponseDto>.Fail("Credenciales inválidas.");

                if (user.RoleId != 1  && user.RoleId != 2)
                    return ApiResponse<LoginResponseDto>.Fail("No eres cliente.");


                if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                    return ApiResponse<LoginResponseDto>.Fail("Credenciales inválidas.");

                var token = _jwtSecurityService.GenerateJwtToken(user);

                var responseDto = new LoginResponseDto { Token = token, UserName = user.FullName, Role = user.Role.Name, Email = user.Email, PhoneNumber = user.PhoneNumber};

                return ApiResponse<LoginResponseDto>.Ok(responseDto, "Login exitoso.");
            }
            catch
            {
                return ApiResponse<LoginResponseDto>.Fail("Ocurrió un error durante el login.");
            }
        }

        public async Task<ApiResponse<UserDto>> RegisterAsync(RegisterUserDto userReceived)
        {
            try
            {
                var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    string normalizedEmail = userReceived.Email.Trim().ToLowerInvariant();

                    var role = await _unitOfWork.Roles.GetByItem(r => r.Id == userReceived.RoleId);
                    if (role == null)
                        return null;

                    var existingUser = await _unitOfWork.Users.GetByItem(
                        u => u.Email == normalizedEmail,
                        include: q => q.Include(u => u.Role)
                    );

                    if (existingUser != null)
                        return null;

                    var userForCreate = _mapper.Map<User>(userReceived);
                    userForCreate.Email = normalizedEmail;
                    userForCreate.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userReceived.Password);

                    await _unitOfWork.Users.Create(userForCreate);
                    await _unitOfWork.SaveChangesAsync();

                    var fullUser = await _unitOfWork.Users.GetByItem(
                        u => u.Id == userForCreate.Id,
                        include: q => q.Include(u => u.Role)
                    );

                    return fullUser;
                });

                if (result == null)
                    return ApiResponse<UserDto>.Fail("Error al registrar el usuario (rol inválido o ya existe).");

                var userDto = _mapper.Map<UserDto>(result);
                return ApiResponse<UserDto>.Ok(userDto, "Usuario registrado correctamente.");
            }
            catch
            {
                return ApiResponse<UserDto>.Fail("Ocurrió un error al registrar el usuario.");
            }
        }

        public async Task<ApiResponse<UserDto>> UpdateById(int id, UserDto updateDto)
        {
            try
            {
                var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var existingUser = await _unitOfWork.Users.GetByItem(
                        u => u.Id == id,
                        include: q => q.Include(u => u.Role)
                    );

                    if (existingUser == null)
                        return null;

                    existingUser.FullName = updateDto.FullName?.Trim() ?? existingUser.FullName;
                    existingUser.Email = updateDto.Email?.Trim().ToLowerInvariant() ?? existingUser.Email;
                    existingUser.RoleId = updateDto.RoleId ?? existingUser.RoleId;

                    await _unitOfWork.Users.Edit(existingUser);
                    await _unitOfWork.SaveChangesAsync();

                    var fullUser = await _unitOfWork.Users.GetByItem(
                        u => u.Id == existingUser.Id,
                        include: q => q.Include(u => u.Role)
                    );

                    return fullUser;
                });

                if (result == null)
                    return ApiResponse<UserDto>.Fail("El usuario no existe.");

                var userDto = _mapper.Map<UserDto>(result);
                return ApiResponse<UserDto>.Ok(userDto, "Usuario editado correctamente.");
            }
            catch
            {
                return ApiResponse<UserDto>.Fail("Ocurrió un error al editar el usuario.");
            }
        }

        public async Task<ApiResponse<UserDto>> UpdateByIdForClient(int id, RegisterUserDto updateUser)
        {
            try
            {
                var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var existingUser = await _unitOfWork.Users.GetByItem(
                        u => u.Id == id,
                        include: q => q.Include(u => u.Role)
                    );

                    if (existingUser == null)
                        return null;

                    existingUser.FullName = updateUser.FullName?.Trim() ?? existingUser.FullName;
                    existingUser.Email = updateUser.Email?.Trim().ToLowerInvariant() ?? existingUser.Email;
                    existingUser.RoleId = 2;
                    if(updateUser.Password != null)
                        existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUser.Password);

                    await _unitOfWork.Users.Edit(existingUser);
                    await _unitOfWork.SaveChangesAsync();

                    var fullUser = await _unitOfWork.Users.GetByItem(
                        u => u.Id == existingUser.Id,
                        include: q => q.Include(u => u.Role)
                    );

                    return fullUser;
                });

                if (result == null)
                    return ApiResponse<UserDto>.Fail("El usuario no existe.");

                var userDto = _mapper.Map<UserDto>(result);
                return ApiResponse<UserDto>.Ok(userDto, "Usuario editado correctamente.");
            }
            catch
            {
                return ApiResponse<UserDto>.Fail("Ocurrió un error al editar el usuario.");
            }

        }
    }
}
