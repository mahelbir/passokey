using System.ComponentModel.DataAnnotations;

namespace Application.Models.PasskeyRegistration;

public class StartPasskeyRegistrationRequest
{
    [StringLength(255, MinimumLength = 3)]
    public string? Username { get; set; }
}