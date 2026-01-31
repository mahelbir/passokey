using Application.Models.General.ViewModel;
using Application.Persistence.User;

namespace Application.Models.User;

public class ListUserViewModel : ListViewModel<UserEntity>
{
    public Guid? AdminUserId { get; set; }
    public Guid? PermissionClientId { get; set; }
}
