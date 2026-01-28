using System.ComponentModel.DataAnnotations;
using Application.Models.Attributes;

namespace Application.Models.General.Request;

public class GetRequest
{
    [Required]
    [Min(1)]
    public int Id { get; set; }
}

public class GuidGetRequest
{
    [Required]
    [GuidDataType]
    public string Id { get; set; }
}