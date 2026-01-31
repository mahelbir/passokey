using System.ComponentModel.DataAnnotations;
using Application.Models.Attributes;

namespace Application.Models.PasskeyLogin;

public class StartPasskeyLoginRequest
{
    [Required]
    [GuidDataType]
    public Guid ClientId { get; set; }
}