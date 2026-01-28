using System.ComponentModel.DataAnnotations;
using Application.Models.Attributes;

namespace Application.Models.General.Request;

public class PaginateRequest
{
    [Min(1)]
    public int PageNumber { get; set; } = 1;
    
    [Range(1, 100)]
    public int PageSize { get; set; } = 25;
    
    [Min(1)]
    public int ButtonCount { get; set; } = 5;
}