using Application.Models.General.Request;
using Application.Models.General.Response;
using Application.Persistence.Client;

namespace Application.Models.UserClientPermissions;

public class ListUserClientPermissionViewModel
{
    public SearchablePaginateRequest Request { get; set; } = null!;
    public PaginateResponse<UserClientPermissionItem> Pagination { get; set; } = null!;
    public Guid? AdminUserId { get; set; }
    public ClientEntity Client { get; set; }
}
