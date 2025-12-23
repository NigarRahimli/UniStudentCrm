using Microsoft.EntityFrameworkCore;
using StudentCrm.Application.Abstract.Repositories;
using StudentCrm.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Repositories
{
    public class WriteRepository<T> : IWriteRepository<T> where T : class, new()
    {
        private readonly StudentCrmDbContext _context;

        public WriteRepository(StudentCrmDbContext context)
        {
            _context = context;
        }

        private DbSet<T> Table { get => _context.Set<T>(); }
        public async Task AddAsync(T entity)
        {
            await Table.AddAsync(entity);
        }

        public async Task HardDeleteAsync(T entity)
        {
            await Task.Run(() => Table.Remove(entity));

        }
        public DbContext GetDbContext()
        {
            return _context;
        }
        public async Task SoftDeleteAsync(T entity)
        {
            // Stub nesneyi attach et
            Table.Attach(entity);

            // Sadece IsDeleted alanını güncelle
            var entry = Table.Entry(entity);
            entry.Property("IsDeleted").CurrentValue = true;
            entry.Property("IsDeleted").IsModified = true;

            await Task.CompletedTask;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            await Task.Run(() => Table.Update(entity));
            return entity;
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
