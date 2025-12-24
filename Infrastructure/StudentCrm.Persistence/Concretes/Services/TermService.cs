using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Student.Domain.Entities;
using StudentCrm.Application.Abstract.Repositories.Sections;
using StudentCrm.Application.Abstract.Repositories.Terms;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.DTOs.Term;
using StudentCrm.Application.GlobalAppException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Services
{
    public class TermService : ITermService
    {
        private readonly ITermReadRepository _termReadRepo;
        private readonly ITermWriteRepository _termWriteRepo;
        private readonly ISectionReadRepository _sectionReadRepo;
        private readonly ISectionWriteRepository _sectionWriteRepo;
        private readonly IMapper _mapper;

        public TermService(
            ITermReadRepository termReadRepo,
            ITermWriteRepository termWriteRepo,
            ISectionReadRepository sectionReadRepo,
            ISectionWriteRepository sectionWriteRepo,
            IMapper mapper)
        {
            _termReadRepo = termReadRepo;
            _termWriteRepo = termWriteRepo;
            _sectionReadRepo = sectionReadRepo;
            _sectionWriteRepo = sectionWriteRepo;
            _mapper = mapper;
        }

        public async Task<List<TermDto>> GetAllAsync()
        {
            var terms = await _termReadRepo.GetAllAsync(
                t => !EF.Property<bool>(t, "IsDeleted"),
                include: q => q.Include(t => t.Sections),
                enableTracking: false
            );

            return _mapper.Map<List<TermDto>>(terms.ToList());
        }

        public async Task<TermDto> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var termId))
                throw new GlobalAppException("Invalid term ID!");

            var term = await _termReadRepo.GetAsync(
                t => t.Id == termId && !EF.Property<bool>(t, "IsDeleted"),
                include: q => q.Include(t => t.Sections),
                enableTracking: false
            );

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

            var name = dto.Name.Trim();

            // unique name
            var exists = await _termReadRepo.GetAsync(
                t => t.Name == name && !EF.Property<bool>(t, "IsDeleted"),
                enableTracking: false
            );

            if (exists != null)
                throw new GlobalAppException("A term with this name already exists!");

            var term = new Term
            {
                Name = name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            await _termWriteRepo.AddAsync(term);
            await _termWriteRepo.CommitAsync();
        }

        public async Task UpdateAsync(UpdateTermDto dto)
        {
            if (dto == null)
                throw new GlobalAppException("Invalid term data!");

            if (string.IsNullOrWhiteSpace(dto.Id) || !Guid.TryParse(dto.Id, out var termId))
                throw new GlobalAppException("Invalid term ID!");

            var term = await _termReadRepo.GetAsync(
                t => t.Id == termId && !EF.Property<bool>(t, "IsDeleted"),
                enableTracking: true
            );

            if (term == null)
                throw new GlobalAppException("Term not found!");

            // name optional update
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var newName = dto.Name.Trim();

                if (!string.Equals(term.Name, newName, StringComparison.OrdinalIgnoreCase))
                {
                    var other = await _termReadRepo.GetAsync(
                        t => t.Name == newName && t.Id != termId && !EF.Property<bool>(t, "IsDeleted"),
                        enableTracking: false
                    );

                    if (other != null)
                        throw new GlobalAppException("A term with this name already exists!");
                }

                term.Name = newName;
            }

            term.StartDate = dto.StartDate;
            term.EndDate = dto.EndDate;

            if (term.EndDate <= term.StartDate)
                throw new GlobalAppException("End date must be after start date!");

            await _termWriteRepo.UpdateAsync(term);
            await _termWriteRepo.CommitAsync();
        }

        public async Task DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var termId))
                throw new GlobalAppException("Invalid term ID!");

            var term = await _termReadRepo.GetAsync(
                t => t.Id == termId && !EF.Property<bool>(t, "IsDeleted"),
                enableTracking: true
            );

            if (term == null)
                throw new GlobalAppException("Term not found!");

            // unlink sections that reference this term
            var sections = await _sectionReadRepo.GetAllAsync(
                s => s.TermId == termId && !EF.Property<bool>(s, "IsDeleted"),
                enableTracking: true
            );

            foreach (var sec in sections)
            {
                // If your TermId is non-nullable Guid, you currently use Guid.Empty.
                // Better design is Guid? TermId and set null.
                sec.TermId = Guid.Empty;
                await _sectionWriteRepo.UpdateAsync(sec);
            }

            // delete term:
            // Hard delete:
            // await _termWriteRepo.HardDeleteAsync(term);

            // Soft delete (recommended):
            await _termWriteRepo.SoftDeleteAsync(term);

            await _sectionWriteRepo.CommitAsync();
            await _termWriteRepo.CommitAsync();
        }
    }
}
