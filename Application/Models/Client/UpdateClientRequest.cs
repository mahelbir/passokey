using System.ComponentModel.DataAnnotations;
using Application.Models.Attributes;

namespace Application.Models.Client;

public class UpdateClientRequest
{
    [Required] [GuidDataType] public Guid Id { get; set; }

    [MaxLength(255)] [MinLength(2)] public string? Name { get; set; }

    public string? IsRegistrationEnabled { get; set; }

    public List<string>? RedirectUriList { get; set; }
}