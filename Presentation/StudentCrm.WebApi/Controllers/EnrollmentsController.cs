using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Enrollment;
using StudentCrm.Application.GlobalAppException;

namespace StudentCrm.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ILogger<EnrollmentsController> _logger;

        public EnrollmentsController(IEnrollmentService enrollmentService, ILogger<EnrollmentsController> logger)
        {
            _enrollmentService = enrollmentService;
            _logger = logger;
        }
        [Authorize(Roles = "Admin,Coordinator,Teacher")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _enrollmentService.GetAllAsync();
                return Ok(new { StatusCode = 200, Data = result });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Enrollments gətirilərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Enrollments gətirilərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }
        [Authorize(Roles = "Admin,Coordinator,Teacher")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById( string id)
        {
            try
            {
                var result = await _enrollmentService.GetByIdAsync(id);
                return Ok(new { StatusCode = 200, Data = result });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Enrollment gətirilərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Enrollment gətirilərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        [Authorize(Roles = "Admin,Coordinator")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEnrollmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { StatusCode = 400, Error = "Yanlış daxil edilmə məlumatı!" });

            try
            {
                await _enrollmentService.CreateAsync(dto);
                return StatusCode(201, new { StatusCode = 201, Message = "Enrollment uğurla yaradıldı!" });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Enrollment yaradılarkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Enrollment yaradılarkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        [Authorize(Roles = "Admin,Coordinates")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateEnrollmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { StatusCode = 400, Error = "Yanlış daxil edilmə məlumatı!" });

            try
            {
                await _enrollmentService.UpdateAsync(dto);
                return Ok(new { StatusCode = 200, Message = "Enrollment uğurla yeniləndi!" });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Enrollment yenilənərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Enrollment yenilənərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _enrollmentService.DeleteAsync(id);
                return Ok(new { StatusCode = 200, Message = "Enrollment uğurla silindi!" });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Enrollment silinərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Enrollment silinərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }
    }
}
