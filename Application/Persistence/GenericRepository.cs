using System.Linq.Expressions;
using Application.Models.General;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence;

public class GenericRepository<T>(AppDbContext context) where T : Entity
{
    protected readonly AppDbContext Context = context;

    protected readonly DbSet<T> DbSet = context.Set<T>();

    public IQueryable<T> Query() => DbSet.AsQueryable();

    public IQueryable<T> Query(Expression<Func<T, bool>> predicate) => DbSet.Where(predicate);

    public Task<T?> Get(Expression<Func<T, bool>> predicate) => DbSet.Where(predicate).FirstOrDefaultAsync();

    public Task<T?> GetById(Guid id) => DbSet.Where(e => e.Id == id).FirstOrDefaultAsync();

    public async Task<T> Create(T entity)
    {
        await DbSet.AddAsync(entity);
        return entity;
    }

    public void Update(T entity) => DbSet.Update(entity);

    public void Delete(T entity) => DbSet.Remove(entity);

    public Task<int> Count(
        IQueryable<T>? query = null
    )
    {
        query ??= Query();
        return query
            .Select(u => u.Id)
            .CountAsync();
    }

    protected Task<List<T>> GetPaginated(
        IQueryable<T> query,
        int limit,
        int offset,
        List<SortModel>? sort = null,
        string[]? sortableFields = null
    )
    {
        if (sort != null && sortableFields != null)
        {
            var orderedQuery = query.OrderBy(u => 0);
            foreach (var s in sort)
            {
                if (string.IsNullOrWhiteSpace(s.Field) || !sortableFields.Contains(s.Field))
                {
                    continue;
                }

                if (s.Direction == SortDirection.Asc)
                {
                    orderedQuery = orderedQuery.ThenBy(u => EF.Property<object>(u, s.Field));
                }
                else
                {
                    orderedQuery = orderedQuery.ThenByDescending(u => EF.Property<object>(u, s.Field));
                }
            }

            query = orderedQuery;
        }

        return query
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    protected IQueryable<T> CreatePaginationQuery(string? searchQuery = null)
    {
        return Query();
    }
}