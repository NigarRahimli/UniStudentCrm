using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Student;
using StudentCrm.Application.GlobalAppException;
using StudentCrm.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class StudentService : IStudentService
    {
        private readonly StudentCrmDbContext _db;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public StudentService(
            StudentCrmDbContext db,
            IMapper mapper,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            _db = db;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<StudentDto>> GetAllAsync()
        {
            var students = await _db.Students.ToListAsync();
            return _mapper.Map<List<StudentDto>>(students);
        }

        public async Task<StudentDetailDto> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var studentId))
                throw new GlobalAppException("Invalid student ID!");

            var student = await _db.Students
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Section)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                throw new GlobalAppException("Student not found!");

            return _mapper.Map<StudentDetailDto>(student);
        }

        public async Task CreateAsync(CreateStudentDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Submitted data is null!");

            if (string.IsNullOrWhiteSpace(dto.StudentNo))
                throw new GlobalAppException("StudentNo cannot be empty!");

            if (await _db.Students.AnyAsync(s => s.StudentNo == dto.StudentNo))
                throw new GlobalAppException("StudentNo must be unique!");

            var student = new StudentUser
            {
                StudentNo = dto.StudentNo.Trim(),
                Name = dto.Name.Trim(),
                Surname = dto.Surname.Trim(),
                Phone = dto.Phone?.Trim(),
                Email = dto.Email?.Trim()
            };

            _db.Students.Add(student);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(UpdateStudentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Id) ||
                !Guid.TryParse(dto.Id, out var studentId))
                throw new GlobalAppException("Invalid student ID!");

            var student = await _db.Students.FindAsync(studentId);
            if (student == null)
                throw new GlobalAppException("Student not found!");

            if (!string.IsNullOrWhiteSpace(dto.StudentNo))
            {
                if (await _db.Students.AnyAsync(s => s.StudentNo == dto.StudentNo && s.Id != studentId))
                    throw new GlobalAppException("Another student with this StudentNo already exists!");
                student.StudentNo = dto.StudentNo.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                student.Name = dto.Name.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Surname))
                student.Surname = dto.Surname.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Phone))
                student.Phone = dto.Phone.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Email))
                student.Email = dto.Email.Trim();

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var studentId))
                throw new GlobalAppException("Invalid student ID!");

            var student = await _db.Students.FindAsync(studentId);
            if (student == null)
                throw new GlobalAppException("Student not found!");

            _db.Students.Remove(student);
            await _db.SaveChangesAsync();
        }

        public async Task AddLoginAsync(string id, AddStudentLoginDto dto)
        {
            if (!Guid.TryParse(id, out var studentId))
                throw new GlobalAppException("Invalid student ID!");

            if (dto == null)
                throw new GlobalAppException("Submitted data is null!");

            var student = await _db.Students.FindAsync(studentId);
            if (student == null)
                throw new GlobalAppException("Student not found!");

            if (student.AppUserId != null)
                throw new GlobalAppException("Login already created for this student.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new GlobalAppException("Email cannot be empty!");

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new GlobalAppException("Password cannot be empty!");

            // Create identity user
            var user = new AppUser
            {
                UserName = dto.Email.Trim(),
                Email = dto.Email.Trim(),
                Name = student.Name,
                Surname = student.Surname,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new GlobalAppException("Login creation failed!");

            // Ensure 'Student' role exists
            const string role = "Student";
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new AppRole { Name = role });

            await _userManager.AddToRoleAsync(user, role);

            // Link login to student
            student.AppUserId = user.Id;
            await _db.SaveChangesAsync();
        }
    }
}
