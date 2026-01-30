using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.User;

public class UserRepository(AppDbContext context) : GenericRepository<UserEntity>(context)
{
    public Task<bool> IsExists(Expression<Func<UserEntity, bool>> predicate)
    {
        return Get(predicate).AnyAsync();
    }
}
