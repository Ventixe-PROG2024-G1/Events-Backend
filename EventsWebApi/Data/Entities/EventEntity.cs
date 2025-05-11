using EventsWebApi.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventsWebApi.Data.Entities;

public class EventEntity
{
    [Key]
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string EventName { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? EventImageId { get; set; }

    [ForeignKey(nameof(Category))]
    public Guid CategoryId { get; set; }
    public CategoryEntity Category { get; set; } = null!;

    [Column(TypeName = "datetime2(0)")]
    public DateTime EventStartDate { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime EventEndDate { get; set; }
    public EventStatus Status { get; set; }

    public Guid? LocationId { get; set; }
}
