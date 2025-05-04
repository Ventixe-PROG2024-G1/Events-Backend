using EventsWebApi.Domain;
using System.ComponentModel.DataAnnotations;

namespace EventsWebApi.ApiModels.Requests;

public class UpdateEventRequest
{
    public Guid Id { get; set; }

    public Guid? EventImageId { get; set; }
    public string? EventName { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime? EventStartDate { get; set; }
    public DateTime? EventEndDate { get; set; }

    [EnumDataType(typeof(EventStatus))]
    public EventStatus? Status { get; set; }
}
