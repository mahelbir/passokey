using System.ComponentModel.DataAnnotations;
using Fido2NetLib;

namespace Application.Models.PasskeyLogin;

public class FinishPasskeyLoginRequest
{
    [Required]
    public AuthenticatorAssertionRawResponse Credential { get; set; }

    public string? State { get; set; }
    public string? RedirectUri { get; set; }
}