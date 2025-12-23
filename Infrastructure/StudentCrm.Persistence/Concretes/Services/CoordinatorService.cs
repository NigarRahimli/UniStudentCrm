using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Coordinator;
using StudentCrm.Application.GlobalAppException;
using StudentCrm.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class CoordinatorService : ICoordinatorService
    {
        private readonly StudentCrmDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;

        public CoordinatorService(
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

        public async Task<List<CoordinatorDto>> GetAllAsync()
        {
            var list = await _db.Coordinators
                .Include(c => c.AppUser)
                .ToListAsync();

            return _mapper.Map<List<CoordinatorDto>>(list);
        }

        public async Task<CoordinatorDto> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var coordinatorId))
                throw new GlobalAppException("Invalid Coordinator ID!");

            var entity = await _db.Coordinators
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(c => c.Id == coordinatorId);

            if (entity == null)
                throw new GlobalAppException("Coordinator not found!");

            return _mapper.Map<CoordinatorDto>(entity);
        }

        public async Task CreateAsync(CreateCoordinatorDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Submitted data is null!");
            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new GlobalAppException("FullName cannot be empty!");
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new GlobalAppException("Email cannot be empty!");
            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new GlobalAppException("Password cannot be empty!");

            // Check unique CoordinatorNo
            if (await _db.Coordinators.AnyAsync(x => x.CoordinatorNo == dto.CoordinatorNo))
                throw new GlobalAppException("Coordinator number must be unique!");

            // Create Identity user
            var user = new AppUser
            {
                UserName = dto.Email.Trim(),
                Email = dto.Email.Trim(),
                EmailConfirmed = true,
                Name = dto.FullName.Trim(),
                Surname = ""
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new GlobalAppException(string.Join("; ", result.Errors.Select(e => e.Description)));

            // ensure coordinator role
            const string role = "Coordinator";
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new AppRole { Name = role });

            await _userManager.AddToRoleAsync(user, role);

            // create domain coordinator
            var coordinator = new CoordinatorUser
            {
                FullName = dto.FullName.Trim(),
                Department = dto.Department?.Trim(),
                CoordinatorNo = dto.CoordinatorNo,
                AppUserId = user.Id
            };

            _db.Coordinators.Add(coordinator);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(UpdateCoordinatorDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Invalid Coordinator data!");

            if (string.IsNullOrWhiteSpace(dto.Id) || !Guid.TryParse(dto.Id, out var coordinatorId))
                throw new GlobalAppException("Invalid Coordinator ID!");

            var existing = await _db.Coordinators
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(c => c.Id == coordinatorId);

            if (existing == null)
                throw new GlobalAppException("Coordinator not found!");

            // unique CoordinatorNo (if changed)
            if (dto.CoordinatorNo.HasValue && dto.CoordinatorNo.Value != existing.CoordinatorNo)
            {
                bool noExists = await _db.Coordinators.AnyAsync(x =>
                    x.CoordinatorNo == dto.CoordinatorNo.Value && x.Id != coordinatorId);

                if (noExists)
                    throw new GlobalAppException("Coordinator number must be unique!");

                existing.CoordinatorNo = dto.CoordinatorNo.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                existing.FullName = dto.FullName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Department))
                existing.Department = dto.Department.Trim();

            // update identity email if changed
            if (!string.IsNullOrWhiteSpace(dto.Email) &&
                existing.AppUser != null &&
                existing.AppUser.Email != dto.Email.Trim())
            {
                existing.AppUser.Email = dto.Email.Trim();
                existing.AppUser.UserName = dto.Email.Trim();
                await _userManager.UpdateAsync(existing.AppUser);
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var coordinatorId))
                throw new GlobalAppException("Invalid Coordinator ID!");

            var entity = await _db.Coordinators.FindAsync(coordinatorId);
            if (entity == null)
                throw new GlobalAppException("Coordinator not found!");

            // remove coordinator
            _db.Coordinators.Remove(entity);
            await _db.SaveChangesAsync();

            // also delete linked identity user
            var user = await _userManager.FindByIdAsync(entity.AppUserId.ToString());
            if (user != null)
                await _userManager.DeleteAsync(user);
        }
    }
}
