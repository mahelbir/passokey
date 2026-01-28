using Application.Persistence.Client;
using Application.Persistence.User;

namespace Application.Persistence.UserClientPermission;

public class UserClientPermissionEntity : Entity
{
    public Guid UserId { get; set; }
    public Guid ClientId { get; set; }

    public UserEntity? User { get; set; }
    public ClientEntity? Client { get; set; }

}
