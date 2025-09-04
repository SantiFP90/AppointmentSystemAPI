using AppointmentSystem.Application.DTOS.WorkingDay;
using AppointmentSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkingDayController : ControllerBase
    {
        private readonly IWorkingDayService _workingDayService;

        public WorkingDayController(IWorkingDayService workingDayService)
        {
            _workingDayService = workingDayService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] WorkingDayCreateDto dto)
        {
            var response = await _workingDayService.CreateAsync(dto);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPatch("update")]
        public async Task<IActionResult> Update([FromQuery] int id, [FromBody] WorkingDayDto dto)
        {
            var response = await _workingDayService.UpdateAsync(id, dto);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var response = await _workingDayService.DeleteAsync(id);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("getById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var response = await _workingDayService.GetByIdAsync(id);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("getAllPaged")]
        public async Task<IActionResult> GetAllPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _workingDayService.GetAllPagedAsync(page, pageSize);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

    }
}
