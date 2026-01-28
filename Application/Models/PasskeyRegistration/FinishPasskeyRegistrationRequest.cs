using System.ComponentModel.DataAnnotations;
using Fido2NetLib;

namespace Application.Models.PasskeyRegistration;

public class FinishPasskeyRegistrationRequest
{
    [Required]
    public AuthenticatorAttestationRawResponse Credential { get; set; } = null!;
}
