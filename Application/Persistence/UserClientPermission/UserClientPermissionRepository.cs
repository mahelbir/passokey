using Application.Models.General.Request;
using Application.Models.UserClientPermissions;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.UserClientPermission;

public class UserClientPermissionRepository(AppDbContext context)
    : GenericRepository<UserClientPermissionEntity>(context)
{
    public IQueryable<UserClientPermissionItem> CreatePaginationQuery(Guid clientId, string? search = null)
    {
        var q = Query(p => p.ClientId == clientId)
            .Include(p => p.User)
            .Select(p => new UserClientPermissionItem
            {
                PermissionId = p.Id,
                UserId = p.User!.Id,
                Username = p.User.Username
            });

        if (!string.IsNullOrEmpty(search))
        {
            q = q.Where(p => p.Username.ToLower().Contains(search.ToLower()));
            if (search.Length > 32 && Guid.TryParse(search, out var guid))
                q = q.Union(Query(p => p.ClientId == clientId && p.UserId == guid)
                    .Include(p => p.User)
                    .Select(p => new UserClientPermissionItem
                    {
                        PermissionId = p.Id,
                        UserId = p.User!.Id,
                        Username = p.User.Username
                    }));
        }

        return q;
    }

    public Task<List<UserClientPermissionItem>> GetPaginated(IQueryable<UserClientPermissionItem> query,
        SearchablePaginateRequest request)
    {
        return query
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();
    }

    public Task<int> Count(IQueryable<UserClientPermissionItem> query)
    {
        return query.CountAsync();
    }

    public Task<bool> IsExists(UserClientPermissionEntity permission)
    {
        return Query(p =>
            p.UserId == permission.UserId &&
            p.ClientId == permission.ClientId
        ).AnyAsync();
    }
}