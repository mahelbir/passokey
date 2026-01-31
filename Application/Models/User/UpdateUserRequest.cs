using System.ComponentModel.DataAnnotations;
using Application.Models.Attributes;

namespace Application.Models.User;

public class UpdateUserRequest
{
    [Required]
    [GuidDataType]
    public Guid Id { get; set; }

    [MaxLength(255)]
    [MinLength(3)]
    public string? Username { get; set; }
}
