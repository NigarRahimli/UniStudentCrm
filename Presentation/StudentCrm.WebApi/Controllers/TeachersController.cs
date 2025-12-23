using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Teacher;
using StudentCrm.Application.GlobalAppException;

namespace StudentCrm.WebApi.Controllers
{
    [Route("api/teachers")]
    [ApiController]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly ILogger<TeachersController> _logger;

        public TeachersController(ITeacherService teacherService, ILogger<TeachersController> logger)
        {
            _teacherService = teacherService;
            _logger = logger;
        }

        // GET api/teachers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _teacherService.GetAllAsync();
                return Ok(new { StatusCode = 200, Data = result });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Teacher-lər gətirilərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Teacher-lər gətirilərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        // GET api/teachers/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            try
            {
                var result = await _teacherService.GetByIdAsync(id);
                return Ok(new { StatusCode = 200, Data = result });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Teacher gətirilərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Teacher gətirilərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        // POST api/teachers
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeacherDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { StatusCode = 400, Error = "Yanlış daxil edilmə məlumatı!" });

            try
            {
                await _teacherService.CreateAsync(dto);
                return StatusCode(201, new { StatusCode = 201, Message = "Teacher uğurla yaradıldı!" });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Teacher yaradılarkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Teacher yaradılarkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        // PUT api/teachers/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update( [FromBody] UpdateTeacherDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { StatusCode = 400, Error = "Yanlış daxil edilmə məlumatı!" });

            try
            {
                // DTO-da Id varsa route ilə eyniləşdir:
                // dto.Id = id;

                await _teacherService.UpdateAsync(dto);
                return Ok(new { StatusCode = 200, Message = "Teacher uğurla yeniləndi!" });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Teacher yenilənərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Teacher yenilənərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        // DELETE api/teachers/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            try
            {
                await _teacherService.DeleteAsync(id);
                return Ok(new { StatusCode = 200, Message = "Teacher uğurla silindi!" });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Teacher silinərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Teacher silinərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }
    }
}
