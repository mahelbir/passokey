using Application.Persistence.UserClientPermission;

namespace Application.Persistence.Client;

public class ClientEntity : Entity
{
    public string Name { get; set; }
    public string SecretKey { get; set; }
    public string? RedirectUri { get; set; }
    public bool IsRegistrationEnabled { get; set; }
    public bool IsAdmin { get; set; }

    public ICollection<UserClientPermissionEntity> UserPermissions { get; set; } = new List<UserClientPermissionEntity>();

}