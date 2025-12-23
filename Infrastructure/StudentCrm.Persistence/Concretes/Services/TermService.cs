using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Term;
using StudentCrm.Application.GlobalAppException;
using Student.Domain.Entities;
using StudentCrm.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class TermService : ITermService
    {
        private readonly StudentCrmDbContext _db;
        private readonly IMapper _mapper;

        public TermService(StudentCrmDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<TermDto>> GetAllAsync()
        {
            var terms = await _db.Terms
                .Include(t => t.Sections)
                .ToListAsync();

            return _mapper.Map<List<TermDto>>(terms);
        }

        public async Task<TermDto> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var termId))
                throw new GlobalAppException("Invalid term ID!");

            var term = await _db.Terms
                .Include(t => t.Sections)
                .FirstOrDefaultAsync(t => t.Id == termId);

            if (term == null)
                throw new GlobalAppException("Term not found!");

            return _mapper.Map<TermDto>(term);
        }

        public async Task CreateAsync(CreateTermDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Submitted data is null!");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new GlobalAppException("Name cannot be empty!");

            if (dto.EndDate <= dto.StartDate)
                throw new GlobalAppException("End date must be after start date!");

            // optional: unique term name?
            if (await _db.Terms.AnyAsync(t => t.Name == dto.Name.Trim()))
                throw new GlobalAppException("A term with this name already exists!");

            var term = new Term
            {
                Name = dto.Name.Trim(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            _db.Terms.Add(term);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(UpdateTermDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Invalid term data!");

            if (!Guid.TryParse(dto.Id, out var termId))
                throw new GlobalAppException("Invalid term ID!");

            var term = await _db.Terms.FindAsync(termId);
            if (term == null)
                throw new GlobalAppException("Term not found!");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                term.Name = dto.Name.Trim();

            term.StartDate = dto.StartDate;
            term.EndDate = dto.EndDate;

            if (term.EndDate <= term.StartDate)
                throw new GlobalAppException("End date must be after start date!");

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var termId))
                throw new GlobalAppException("Invalid term ID!");

            var term = await _db.Terms.FindAsync(termId);
            if (term == null)
                throw new GlobalAppException("Term not found!");

            // unlink sections that reference this term
            var sections = await _db.Sections
                .Where(s => s.TermId == termId)
                .ToListAsync();

            foreach (var sec in sections)
                sec.TermId = Guid.Empty; // or null if you made TermId nullable

            _db.Terms.Remove(term);
            await _db.SaveChangesAsync();
        }
    }
}
