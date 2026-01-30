namespace Application.Models.General.Request;

public class SortablePaginateRequest : PaginateRequest
{
    public List<SortModel>? Sort { get; set; }

    public new string ToString(int pageNumber)
    {
        var sortParams = Sort != null && Sort.Count > 0
            ? "&" + string.Join("&",
                Sort.Select((s, i) => $"sort[{i}.field={s.Field}&sort[{i}].direction={(int)s.Direction}"))
            : "";
        return $"{base.ToString(pageNumber)}{sortParams}";
    }
}