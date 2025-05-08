using EventsWebApi.Domain;
using System.ComponentModel.DataAnnotations;

namespace EventsWebApi.ApiModels.Requests;

public class CreateEventRequest
{
    [Required]
    public string EventName { get; set; } = null!;
    public string? Description { get; set; }

    public Guid? EventImageId { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime EventStartDate { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime EventEndDate { get; set; }

    [Required]
    [EnumDataType(typeof(EventStatus))]
    public EventStatus Status { get; set; } = EventStatus.Draft;
    public Guid? LocationId { get; set; }
}
