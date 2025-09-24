using AppointmentSystem.Application.DTOS.Appoiment;
using AppointmentSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto)
        {
            var response = await _appointmentService.CreateAsync(dto);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPatch("update")]
        public async Task<IActionResult> Update([FromQuery] int id, [FromBody] AppointmentUpdateDto dto)
        {
            var response = await _appointmentService.UpdateAsync(id, dto);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var response = await _appointmentService.DeleteAsync(id);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("getById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var response = await _appointmentService.GetByIdAsync(id);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("getAllPaged")]
        public async Task<IActionResult> GetAllPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
            [FromQuery] string name = null)
        {
            var response = await _appointmentService.GetAllPagedAsync(page, pageSize, name);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("getByUserId")]
        public async Task<IActionResult> GetByUserId([FromQuery] int userId)
        {
            var response = await _appointmentService.GetByUserIdAsync(userId);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("getByDate")]
        public async Task<IActionResult> GetByDate([FromQuery] DateTime date)
        {
            var response = await _appointmentService.GetByDateAsync(date);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("getAppoimentByMonth")]
        public async Task<IActionResult> GetAppoimentByMonth([FromQuery] int month)
        {
            var response = await _appointmentService.GetAppoimentByMonth(month);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("getAppoimentByMonthYear")]
        public async Task<IActionResult> GetCalendarByMonthAsync([FromQuery] int month, int year)
        {
            var response = await _appointmentService.GetCalendarByMonthAsync(month, year);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("getCounters")]
        public async Task<IActionResult> GetCounters()
        {
            var response = await _appointmentService.GetCounters();
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

    }
}

