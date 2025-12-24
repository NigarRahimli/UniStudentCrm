using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Coordinator;
using StudentCrm.Application.GlobalAppException;

namespace StudentCrm.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoordinatorsController : ControllerBase
    {
        private readonly ICoordinatorService _coordinatorService;
        private readonly ILogger<CoordinatorsController> _logger;

        public CoordinatorsController(
            ICoordinatorService coordinatorService,
            ILogger<CoordinatorsController> logger)
        {
            _coordinatorService = coordinatorService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _coordinatorService.GetAllAsync();
                return Ok(new { StatusCode = 200, Data = result });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Coordinators gətirilərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Coordinators gətirilərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var result = await _coordinatorService.GetByIdAsync(id);
                return Ok(new { StatusCode = 200, Data = result });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Coordinator gətirilərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Coordinator gətirilərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCoordinatorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { StatusCode = 400, Error = "Yanlış daxil edilmə məlumatı!" });

            try
            {
                await _coordinatorService.CreateAsync(dto);
                return StatusCode(201, new { StatusCode = 201, Message = "Coordinator uğurla yaradıldı!" });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Coordinator yaradılarkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Coordinator yaradılarkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCoordinatorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { StatusCode = 400, Error = "Yanlış daxil edilmə məlumatı!" });

            try
            {
                await _coordinatorService.UpdateAsync(dto);
                return Ok(new { StatusCode = 200, Message = "Coordinator uğurla yeniləndi!" });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Coordinator yenilənərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Coordinator yenilənərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _coordinatorService.DeleteAsync(id);
                return Ok(new { StatusCode = 200, Message = "Coordinator uğurla silindi!" });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Coordinator silinərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Coordinator silinərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

    }
}
