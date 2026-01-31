using System.ComponentModel.DataAnnotations;
using Application.Models.Attributes;
using Fido2NetLib;

namespace Application.Models.PasskeyRegistration;

public class FinishPasskeyRegistrationRequest
{
    [Required]
    [GuidDataType]
    public Guid ClientId { get; set; }

    [Required]
    public AuthenticatorAttestationRawResponse Credential { get; set; } = null!;
}
