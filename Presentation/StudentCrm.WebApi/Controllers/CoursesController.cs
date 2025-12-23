using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Course;
using StudentCrm.Application.GlobalAppException;

namespace StudentCrm.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(ICourseService courseService, ILogger<CoursesController> logger)
        {
            _courseService = courseService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _courseService.GetAllAsync();
                return Ok(new { StatusCode = 200, Data = result });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Courses gətirilərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Courses gətirilərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var result = await _courseService.GetByIdAsync(id);
                return Ok(new { StatusCode = 200, Data = result });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Course gətirilərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Course gətirilərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { StatusCode = 400, Error = "Yanlış daxil edilmə məlumatı!" });

            try
            {
                await _courseService.CreateAsync(dto);
                return StatusCode(201, new { StatusCode = 201, Message = "Course uğurla yaradıldı!" });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Course yaradılarkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Course yaradılarkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { StatusCode = 400, Error = "Yanlış daxil edilmə məlumatı!" });

            try
            {
                await _courseService.UpdateAsync(dto);
                return Ok(new { StatusCode = 200, Message = "Course uğurla yeniləndi!" });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Course yenilənərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Course yenilənərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete( string id)
        {
            try
            {
                await _courseService.DeleteAsync(id);
                return Ok(new { StatusCode = 200, Message = "Course uğurla silindi!" });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Course silinərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Course silinərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }
    }
}
