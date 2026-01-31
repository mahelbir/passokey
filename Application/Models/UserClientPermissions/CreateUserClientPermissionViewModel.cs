using Application.Persistence.Client;
using Application.Persistence.User;

namespace Application.Models.UserClientPermissions;

public class CreateUserClientPermissionViewModel
{
    public ClientEntity Client { get; set; }
    public UserEntity User { get; set; }
}
