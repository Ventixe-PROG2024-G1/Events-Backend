﻿using EventsWebApi.Domain;

namespace EventsWebApi.ApiModels.Responses;

public class EventResponse
{
    public Guid Id { get; set; }
    public Guid? EventImageId { get; set; }
    public string EventName { get; set; } = null!;
    public string? Description { get; set; }
    public CategoryResponse Category { get; set; } = null!;
    public DateTime EventStartDate { get; set; }
    public DateTime EventEndDate { get; set; }
    public EventStatus Status { get; set; }
    public Guid? LocationId { get; set; }
}
