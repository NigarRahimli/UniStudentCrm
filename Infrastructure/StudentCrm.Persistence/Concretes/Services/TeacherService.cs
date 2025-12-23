using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Teacher;
using StudentCrm.Application.GlobalAppException;
using Student.Domain.Entities;
using StudentCrm.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly StudentCrmDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;

        public TeacherService(
            StudentCrmDbContext db,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IMapper mapper)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<List<TeacherDto>> GetAllAsync()
        {
            var teachers = await _db.Teachers
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Course)
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Term)
                .ToListAsync();

            return _mapper.Map<List<TeacherDto>>(teachers);
        }

        public async Task<TeacherDto> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var teacherId))
                throw new GlobalAppException("Invalid teacher ID!");

            var teacher = await _db.Teachers
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Course)
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Term)
                .FirstOrDefaultAsync(t => t.Id == teacherId);

            if (teacher == null)
                throw new GlobalAppException("Teacher not found!");

            return _mapper.Map<TeacherDto>(teacher);
        }

        public async Task CreateAsync(CreateTeacherDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Submitted data is null!");

            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new GlobalAppException("FullName cannot be empty!");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new GlobalAppException("Email cannot be empty!");

            // Unique StaffNo check
            if (await _db.Teachers.AnyAsync(t => t.StaffNo == dto.StaffNo))
                throw new GlobalAppException("A teacher with this StaffNo already exists!");

            // Create identity user
            var user = new AppUser
            {
                UserName = dto.Email.Trim(),
                Email = dto.Email.Trim(),
                Name = dto.FullName.Trim(),
                Surname = ""
            };

            string tempPassword = "Teach123!"; // temporary

            var identityResult = await _userManager.CreateAsync(user, tempPassword);
            if (!identityResult.Succeeded)
                throw new GlobalAppException("Login creation failed!");

            const string roleName = "Teacher";
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new AppRole { Name = roleName });

            await _userManager.AddToRoleAsync(user, roleName);

            // Save teacher
            var teacher = new TeacherUser
            {
                StaffNo = dto.StaffNo,
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim(),
                AppUserId = user.Id
            };

            _db.Teachers.Add(teacher);
            await _db.SaveChangesAsync();

            // Link sections
            if (dto.SectionIds != null && dto.SectionIds.Any())
            {
                foreach (var secIdStr in dto.SectionIds)
                {
                    if (!Guid.TryParse(secIdStr, out var secId))
                        throw new GlobalAppException($"Invalid Section ID: {secIdStr}");

                    var section = await _db.Sections.FindAsync(secId);
                    if (section == null)
                        throw new GlobalAppException($"Section with ID {secIdStr} not found!");

                    section.TeacherId = teacher.Id;
                }
                await _db.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(UpdateTeacherDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Id) ||
                !Guid.TryParse(dto.Id, out var teacherId))
                throw new GlobalAppException("Invalid teacher data!");

            var teacher = await _db.Teachers
                .Include(t => t.Sections)
                .FirstOrDefaultAsync(t => t.Id == teacherId);

            if (teacher == null)
                throw new GlobalAppException("Teacher not found!");

            // Update simple fields
            if (dto.StaffNo != 0)
                teacher.StaffNo = dto.StaffNo;

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                teacher.FullName = dto.FullName.Trim();

            // Update identity email if changed
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                teacher.Email = dto.Email.Trim();
                var user = await _userManager.FindByIdAsync(teacher.AppUserId.ToString());
                if (user != null)
                {
                    user.Email = dto.Email.Trim();
                    user.UserName = dto.Email.Trim();
                    await _userManager.UpdateAsync(user);
                }
            }

            // Update section links
            if (dto.SectionIds != null)
            {
                // Unlink sections no longer in list
                foreach (var existingSec in teacher.Sections.ToList())
                {
                    if (!dto.SectionIds.Contains(existingSec.Id.ToString()))
                        existingSec.TeacherId = Guid.Empty;
                }

                // Link new ones
                foreach (var secIdStr in dto.SectionIds)
                {
                    if (!Guid.TryParse(secIdStr, out var secId))
                        throw new GlobalAppException($"Invalid Section ID: {secIdStr}");

                    if (!teacher.Sections.Any(s => s.Id == secId))
                    {
                        var section = await _db.Sections.FindAsync(secId);
                        if (section == null)
                            throw new GlobalAppException($"Section with ID {secIdStr} not found!");
                        section.TeacherId = teacher.Id;
                    }
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var teacherId))
                throw new GlobalAppException("Invalid teacher ID!");

            var teacher = await _db.Teachers.FindAsync(teacherId);
            if (teacher == null)
                throw new GlobalAppException("Teacher not found!");

            // Remove teacher
            _db.Teachers.Remove(teacher);
            await _db.SaveChangesAsync();

            // Also delete identity user
            var user = await _userManager.FindByIdAsync(teacher.AppUserId.ToString());
            if (user != null)
                await _userManager.DeleteAsync(user);
        }
    }
}
