namespace Application.Persistence.UserClientPermission;

public class UserClientPermissionRepository(AppDbContext context) : GenericRepository<UserClientPermissionEntity>(context)
{
}
