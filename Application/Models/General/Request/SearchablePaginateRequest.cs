namespace Application.Models.General.Request;

public class SearchablePaginateRequest : SortablePaginateRequest
{
    public string? Search { get; set; }

    public new string ToString(int pageNumber)
    {
        var str = base.ToString(pageNumber);
        if (!string.IsNullOrEmpty(Search))
        {
            str += $"&search={Search}";
        }

        return str;
    }

}