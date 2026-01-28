namespace Application.Models.General.Request;

public class ListRequest : PaginateRequest
{
    public List<SearchModel>? Search { get; set; }
    public List<SortModel>? Sort { get; set; }
}