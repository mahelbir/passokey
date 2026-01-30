using Application.Models.General.Request;
using Application.Models.General.Response;

namespace Application.Models.General.ViewModel;

public class ListViewModel<T>
{
    public SearchablePaginateRequest Request { get; set; }
    public PaginateResponse<T> Pagination { get; set; }
}