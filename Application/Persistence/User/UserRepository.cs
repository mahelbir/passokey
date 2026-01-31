using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.User;

public class UserRepository(AppDbContext context) : GenericRepository<UserEntity>(context)
{
    public Task<bool> IsExists(Guid userId, string username)
    {
        return Query(u =>
            u.Id == userId ||
            u.Username == username
        ).AnyAsync();
    }
}