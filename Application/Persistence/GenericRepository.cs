using System.Linq.Expressions;
using Application.Models.General;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence;

public class GenericRepository<T>(AppDbContext context) where T : Entity
{
    protected AppDbContext Context = context;

    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public IQueryable<T> GetAll() => _dbSet.AsQueryable().AsNoTracking();

    public IQueryable<T> Get(Expression<Func<T, bool>> predicate) => _dbSet.Where(predicate);

    public Task<T?> GetById(Guid id) => _dbSet.Where(e => e.Id.Equals(id)).FirstOrDefaultAsync();

    public Task<T?> GetNtById(Guid id) => _dbSet.Where(e => e.Id.Equals(id)).AsNoTracking().FirstOrDefaultAsync();

    public async Task<T> Create(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public void Update(T entity) => _dbSet.Update(entity);

    public void Delete(T entity) => _dbSet.Remove(entity);

    protected Task<List<T>> GetPaginated(
        IQueryable<T> query,
        string[] allowedFields,
        int limit,
        int offset,
        List<SearchModel>? search = null,
        List<SortModel>? sort = null
    )
    {
        if (search != null)
        {
            foreach (var s in search)
            {
                query = QueryUtils<T>.ApplySearchFilter(query, s, allowedFields);
            }
        }

        if (sort != null)
        {
            var orderedQuery = query.OrderBy(u => 0);
            foreach (var s in sort)
            {
                orderedQuery = QueryUtils<T>.ApplySort(orderedQuery, s, allowedFields);
            }

            query = orderedQuery;
        }

        return query
            .Skip(offset)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<int> Count(
        IQueryable<T> query,
        string[]? allowedFields = null,
        List<SearchModel>? search = null
    )
    {
        if (search != null)
        {
            foreach (var s in search)
            {
                query = QueryUtils<T>.ApplySearchFilter(query, s, allowedFields!);
            }
        }

        return query
            .Select(u => u.Id)
            .CountAsync();
    }
}