using System.ComponentModel.DataAnnotations;

namespace Application.Models.Client;

public class CreateClientRequest
{
    [Required]
    [MaxLength(255)]
    [MinLength(2)]
    public string Name { get; set; }

    public string? RedirectUri { get; set; }
}