namespace Application.Models.UserClientPermissions;

public class UserClientPermissionItem
{
    public Guid PermissionId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
}