using System.ComponentModel.DataAnnotations;

namespace EventsWebApi.ApiModels.DTO;

public class GetEventQuery
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    private int _pageNumber = DefaultPageNumber;
    private int _pageSize = DefaultPageSize;

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be 1 or greater.")]
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = (value >= 1) ? value : DefaultPageNumber;
    }

    [Range(1, MaxPageSize, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (value < 1)
            {
                _pageSize = DefaultPageSize;
            }
            else if (value > MaxPageSize)
            {
                _pageSize = MaxPageSize;
            }
            else
            {
                _pageSize = value;
            }
        }
    }

    public string? CategoryNameFilter { get; set; }
    //public Guid? CategoryIdFilter { get; set; }

    public string? SearchTerm { get; set; }
    public string? DateFilter { get; set; }
    public DateTime? SpecificDateFrom { get; set; }
    public DateTime? SpecificDateTo { get; set; }

    public GetEventQuery()
    {
        PageNumber = DefaultPageNumber;
        PageSize = DefaultPageSize;
    }
}
