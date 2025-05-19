namespace EventsWebApi.ApiModels.Responses;

public class PagingEventResult
{
    public List<EventResponse> Events { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => TotalCount > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagingEventResult(List<EventResponse> events, int pageNumber, int pageSize, int totalCount)
    {
        Events = events ?? new List<EventResponse>();
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}
