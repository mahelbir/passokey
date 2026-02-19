using Fido2NetLib;

namespace Application.Models.PasskeyLogin;

public class StartPasskeyLoginResponse
{
    public AssertionOptions Options { get; set; } = null!;
}