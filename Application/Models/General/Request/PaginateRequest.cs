using System.ComponentModel.DataAnnotations;

namespace Application.Models.General.Request;

public class PaginateRequest
{
    private int _pageNumber = 1;
    private int _pageSize = 25;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = Math.Max(1, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, 100);
    }

    [Range(1, 10)]
    public int ButtonCount { get; set; } = 5;

    public int Offset => (PageNumber - 1) * PageSize;

    public string ToString(int pageNumber)
    {
        return $"pageNumber={pageNumber}&pageSize={PageSize}";
    }

    public new string ToString()
    {
        return ToString(PageNumber);
    }

}