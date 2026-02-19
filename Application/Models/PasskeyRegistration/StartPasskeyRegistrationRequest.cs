using System.ComponentModel.DataAnnotations;
using Application.Models.Attributes;

namespace Application.Models.PasskeyRegistration;

public class StartPasskeyRegistrationRequest
{
    [Required] [GuidDataType] public Guid ClientId { get; set; }

    [StringLength(255, MinimumLength = 3)] public string? Username { get; set; }
}