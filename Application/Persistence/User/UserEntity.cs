using Application.Persistence.UserClientPermission;
using Application.Persistence.UserCredential;

namespace Application.Persistence.User;

public class UserEntity : Entity
{
    public string Username { get; set; }

    public ICollection<UserClientPermissionEntity> ClientPermissions { get; set; } =
        new List<UserClientPermissionEntity>();

    public ICollection<UserCredentialEntity> Credentials { get; set; } = new List<UserCredentialEntity>();
}