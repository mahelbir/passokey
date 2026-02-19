using Application.Models.General.Request;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.User;

public class UserRepository(AppDbContext context) : GenericRepository<UserEntity>(context)
{
    public new IQueryable<UserEntity> CreatePaginationQuery(string? search = null)
    {
        var q = Query();
        if (!string.IsNullOrEmpty(search))
        {
            q = q.Where(u => u.Username.ToLower().Contains(search.ToLower()));
            if (search.Length > 32 && Guid.TryParse(search, out var guid))
                q = q.Union(Query().Where(u => u.Id == guid));
        }

        return q;
    }

    public Task<List<UserEntity>> GetPaginated(IQueryable<UserEntity> paginationQuery,
        SearchablePaginateRequest request)
    {
        return GetPaginated(
            paginationQuery,
            request.PageSize,
            request.Offset
        );
    }

    public Task<bool> IsExists(Guid userId, string username)
    {
        return Query(u =>
            u.Id == userId ||
            u.Username == username
        ).AnyAsync();
    }
}