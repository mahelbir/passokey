using Application.Models.General.Request;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.Client;

public class ClientRepository(AppDbContext context) : GenericRepository<ClientEntity>(context)
{
    public Task<List<ClientEntity>> GetPaginated(IQueryable<ClientEntity> paginationQuery,
        SearchablePaginateRequest request)
    {
        return GetPaginated(
            paginationQuery,
            request.PageSize,
            request.Offset,
            request.Sort,
            ["IsRegistrationEnabled"]
        );
    }

    public new IQueryable<ClientEntity> CreatePaginationQuery(string? search = null)
    {
        var q = Query();
        if (!string.IsNullOrEmpty(search))
        {
            q = q.Where(c => c.Name.ToLower().Contains(search.ToLower()));
            if (search.Length > 32 && Guid.TryParse(search, out var guid))
            {
                q = q
                    .Union(Query()
                        .Where(c => c.Id == guid));
            }
        }

        return q;
    }


    public Task<ClientEntity?> GetByRegistrationEnabled(Guid id, bool isRegistrationEnabled = true)
    {
        return Get(c =>
            c.Id == id &&
            c.IsRegistrationEnabled == isRegistrationEnabled
        );
    }
}