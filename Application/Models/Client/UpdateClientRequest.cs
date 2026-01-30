using System.ComponentModel.DataAnnotations;
using Application.Models.Attributes;

namespace Application.Models.Client;

public class UpdateClientRequest
{
    [Required]
    [GuidDataType]
    public Guid Id { get; set; }

    [MaxLength(255)]
    [MinLength(3)]
    public string? Name { get; set; }

    public string? IsRegistrationEnabled { get; set; }

    public string? RedirectUri { get; set; }
}