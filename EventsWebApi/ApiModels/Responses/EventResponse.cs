namespace EventsWebApi.ApiModels.Responses;

public class EventResponse
{
    public Guid Id { get; set; }
    public Guid? EventImageId { get; set; }
    public string EventName { get; set; } = null!;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string Status { get; set; } = null!;
    public CategoryResponse Category { get; set; } = null!;
    public DateTime EventStartDate { get; set; }
    public DateTime EventEndDate { get; set; }
}
