using Microsoft.EntityFrameworkCore;
using StudentCrm.Application.Abstract.Repositories;
using StudentCrm.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StudentCrm.Persistence.Concretes.Repositories
{
    public class ReadRepository<T> : IReadRepository<T> where T : class, new()
    {
        private readonly StudentCrmDbContext _context;

        public ReadRepository(StudentCrmDbContext context)
        {
            _context = context;
        }

        public DbSet<T> Table => _context.Set<T>();

        public async Task<IList<T>> GetAllAsync(
            Expression<Func<T, bool>>? func = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool enableTracking = false)
        {
            IQueryable<T> query = Table;

            if (!enableTracking)
                query = query.AsNoTracking();

            if (include != null)
                query = include(query);

            if (func != null)
                query = query.Where(func);

            if (orderBy != null)
                query = orderBy(query);

            return await query.ToListAsync();
        }

        public async Task<T?> GetAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool enableTracking = false)
        {
            IQueryable<T> query = Table;

            if (!enableTracking)
                query = query.AsNoTracking();

            if (include != null)
                query = include(query);

            query = query.Where(predicate);

            if (orderBy != null)
                query = orderBy(query);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<T> GetByIdAsync(string id, bool enableTracking = false)
        {
            if (!Guid.TryParse(id, out Guid parsedId))
                throw new KeyNotFoundException($"Yanlış ID formatı: {id}");

            IQueryable<T> query = Table;

            if (!enableTracking)
                query = query.AsNoTracking();

            var entity = await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == parsedId);

            if (entity == null)
                throw new KeyNotFoundException($"Entity with ID {id} not found.");

            return entity;
        }

        public async Task<List<T>> GetPagedAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int page = 1,
            int pageSize = 10,
            bool enableTracking = false)
        {
            IQueryable<T> query = Table;

            if (!enableTracking)
                query = query.AsNoTracking();

            if (predicate != null)
                query = query.Where(predicate);

            if (include != null)
                query = include(query);

            if (orderBy != null)
                query = orderBy(query);

            return await query.Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync();
        }

        public Task<int> GetCountAsync(Expression<Func<T, bool>>? func = null)
        {
            IQueryable<T> query = Table.AsNoTracking();
            return func == null ? query.CountAsync() : query.Where(func).CountAsync();
        }

        public IQueryable<T> GetQueryable()
        {
            return Table.AsQueryable();
        }

    
    } 
}
