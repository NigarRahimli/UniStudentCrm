using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Student;
using StudentCrm.Application.GlobalAppException;

namespace StudentCrm.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(IStudentService studentService, ILogger<StudentsController> logger)
        {
            _studentService = studentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _studentService.GetAllAsync();
                return Ok(new { StatusCode = 200, Data = result });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Students gətirilərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Students gətirilərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var result = await _studentService.GetByIdAsync(id);
                return Ok(new { StatusCode = 200, Data = result });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Student gətirilərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Student gətirilərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { StatusCode = 400, Error = "Yanlış daxil edilmə məlumatı!" });

            try
            {
                await _studentService.CreateAsync(dto);
                return StatusCode(201, new { StatusCode = 201, Message = "Student uğurla yaradıldı!" });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Student yaradılarkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Student yaradılarkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateStudentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { StatusCode = 400, Error = "Yanlış daxil edilmə məlumatı!" });

            try
            {
                await _studentService.UpdateAsync( dto);
                return Ok(new { StatusCode = 200, Message = "Student uğurla yeniləndi!" });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Student yenilənərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Student yenilənərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete( string id)
        {
            try
            {
                await _studentService.DeleteAsync(id);
                return Ok(new { StatusCode = 200, Message = "Student uğurla silindi!" });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Student silinərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Student silinərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> AddLogin(string id, AddStudentLoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { StatusCode = 400, Error = "Yanlış daxil edilmə məlumatı!" });

            try
            {
                await _studentService.AddLoginAsync(id, dto);
                return Ok(new { StatusCode = 200, Message = "Student login uğurla əlavə edildi!" });
            }
            catch (GlobalAppException ex)
            {
                _logger.LogError(ex, "Student login əlavə edilərkən xəta baş verdi!");
                return BadRequest(new { StatusCode = 400, Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Student login əlavə edilərkən gözlənilməz xəta baş verdi!");
                return StatusCode(500, new { StatusCode = 500, Error = "Gözlənilməz xəta baş verdi!" });
            }
        }
    }
}
