namespace Application.Models.General.Response;

public interface IPaginateResponse
{
    int PageNumber { get; set; }

    int PageSize { get; set; }

    int Offset { get; }

    bool HasPrevious { get; }

    bool HasNext { get; }

    int ButtonCount { get; set; }

    int ButtonStartPage { get; }

    int ButtonEndPage { get; }

    int TotalPageCount { get; }

    int TotalItemCount { get; set; }
}