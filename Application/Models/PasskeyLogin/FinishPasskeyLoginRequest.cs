using System.ComponentModel.DataAnnotations;
using Application.Models.Attributes;
using Fido2NetLib;

namespace Application.Models.PasskeyLogin;

public class FinishPasskeyLoginRequest
{
    [Required]
    [GuidDataType]
    public Guid ClientId { get; set; }

    [Required]
    public AuthenticatorAssertionRawResponse Credential { get; set; }

    public string? State { get; set; }

    [Url]
    public string? RedirectUri { get; set; }
}