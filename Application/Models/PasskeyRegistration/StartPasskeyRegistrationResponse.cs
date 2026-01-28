using Fido2NetLib;

namespace Application.Models.PasskeyRegistration;

public class StartPasskeyRegistrationResponse
{
    public CredentialCreateOptions Options { get; set; } = null!;
}