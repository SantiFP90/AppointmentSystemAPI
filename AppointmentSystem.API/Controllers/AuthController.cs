using Microsoft.AspNetCore.Http;
using AppointmentSystem.Application.DTOS.Auth;
using AppointmentSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Application.DTOS.User;

namespace AppointmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            var response = await _authService.RegisterAsync(registerDto);

            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("getUserById")]
        public async Task<IActionResult> GetUser([FromQuery] int id )
        {
            var response = await _authService.GetByIdAsync(id);

            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPatch("updateUserById")]
        public async Task<IActionResult> UpdateUser([FromQuery] int id, [FromBody] UserDto userUpdate)
        {
            var response = await _authService.UpdateById(id, userUpdate);

            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPatch("updateUserByIdForClient")]
        public async Task<IActionResult> UpdateUserForClient([FromQuery] int id, [FromBody] RegisterUserDto userUpdate)
        {
            var response = await _authService.UpdateByIdForClient(id, userUpdate);

            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpDelete("deleteUserById")]
        public async Task<IActionResult> DeleteUser([FromQuery] int id)
        {
            var response = await _authService.DeleteById(id);

            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

    }
}
