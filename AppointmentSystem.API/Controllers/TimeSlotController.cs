using AppointmentSystem.Application.DTOS.TimeSlot;
using AppointmentSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeSlotController : ControllerBase
    {
        private readonly ITimeSlotService _timeSlotService;

        public TimeSlotController(ITimeSlotService timeSlotService)
        {
            _timeSlotService = timeSlotService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] TimeSlotDto dto)
        {
            var response = await _timeSlotService.CreateAsync(dto);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPatch("update")]
        public async Task<IActionResult> Update([FromQuery] int id, [FromBody] TimeSlotDto dto)
        {
            var response = await _timeSlotService.UpdateAsync(id, dto);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var response = await _timeSlotService.DeleteAsync(id);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("getById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var response = await _timeSlotService.GetByIdAsync(id);
            if (response != null)
                return Ok(response);
            else
                return NotFound();
        }

        [HttpGet("getAllPaged")]
        public async Task<IActionResult> GetAllPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? workingDayId = null,
            [FromQuery] bool? isAvailable = null
        )
        {
            var response = await _timeSlotService.GetAllAsync(page, pageSize, workingDayId, isAvailable);
            return Ok(response);
        }

        [HttpGet("getAvailableByDay")]
        public async Task<IActionResult> GetAvailableByDay([FromQuery] int workingDayId)
        {
            var response = await _timeSlotService.GetAvailableByDayAsync(workingDayId);
            return Ok(response);
        }
    }
}

