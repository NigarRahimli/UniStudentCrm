using System.Linq.Expressions;


namespace StudentCrm.Application.Abstract.Repositories
{
    public interface IReadRepository<T> where T : class, new()
    {
        Task<T> GetByIdAsync(string id, bool enableTracking = false);

        Task<IList<T>> GetAllAsync(
            Expression<Func<T, bool>>? func = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool enableTracking = false
        );

        Task<T?> GetAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool enableTracking = false
        );

        Task<int> GetCountAsync(Expression<Func<T, bool>>? func = null);

        Task<List<T>> GetPagedAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int page = 1,
            int pageSize = 10,
            bool enableTracking = false
        );
    }
}
