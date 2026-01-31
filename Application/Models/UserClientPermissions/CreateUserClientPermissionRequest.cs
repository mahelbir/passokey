using System.ComponentModel.DataAnnotations;
using Application.Models.Attributes;

namespace Application.Models.UserClientPermissions;

public class CreateUserClientPermissionRequest
{
    [Required]
    [GuidDataType]
    public Guid ClientId { get; set; }

    [Required]
    [GuidDataType]
    public Guid UserId { get; set; }
}