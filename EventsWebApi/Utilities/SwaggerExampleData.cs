using EventsWebApi.ApiModels.DTO;
using EventsWebApi.ApiModels.Requests;
using EventsWebApi.ApiModels.Responses;
using EventsWebApi.Domain;

namespace EventsWebApi.Utilities;

public static class SwaggerExampleData
{
    public static GetEventQuery GetEventQueryExample => new()
    {
        PageNumber = 1,
        PageSize = 10,
        CategoryNameFilter = "Music",
        SearchTerm = "Concert",
        DateFilter = "NextWeek",
        SpecificDateFrom = DateTime.UtcNow.AddDays(7),
        SpecificDateTo = DateTime.UtcNow.AddDays(14),
        StatusFilter = "Active"
    };

    public static CreateEventRequest CreateEventRequestExample => new()
    {
        EventName = "Summer Music Festival",
        Description = "A fantastic lineup of bands and artists.",
        EventImageId = Guid.NewGuid(),
        CategoryId = Guid.NewGuid(),
        EventStartDate = DateTime.UtcNow.AddMonths(2),
        EventEndDate = DateTime.UtcNow.AddMonths(2).AddDays(2),
        Status = EventStatus.Draft,
        LocationId = Guid.NewGuid()
    };

    public static UpdateEventRequest UpdateEventRequestExample => new()
    {
        Id = Guid.NewGuid(),
        EventName = "Updated Summer Music Festival",
        Description = "An updated description with even more exciting details.",
        EventImageId = Guid.NewGuid(),
        CategoryId = Guid.NewGuid(),
        EventStartDate = DateTime.UtcNow.AddMonths(3),
        EventEndDate = DateTime.UtcNow.AddMonths(3).AddDays(2),
        Status = EventStatus.Active,
        LocationId = Guid.NewGuid()
    };

    public static CategoryResponse CategoryResponseExample => new()
    {
        Id = Guid.NewGuid(),
        CategoryName = "Technology"
    };

    public static EventResponse EventResponseExample => new()
    {
        Id = Guid.NewGuid(),
        EventImageId = Guid.NewGuid(),
        EventName = "Tech Conference 2025",
        Description = "Join us for the latest in tech innovation.",
        Category = new CategoryResponse
        {
            Id = Guid.NewGuid(),
            CategoryName = "Technology"
        },
        EventStartDate = DateTime.UtcNow.AddYears(1),
        EventEndDate = DateTime.UtcNow.AddYears(1).AddDays(3),
        Status = EventStatus.Active,
        LocationId = Guid.NewGuid()
    };
}
