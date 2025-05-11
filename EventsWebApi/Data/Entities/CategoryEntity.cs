using System.ComponentModel.DataAnnotations;

namespace EventsWebApi.Data.Entities;

public class CategoryEntity
{
    [Key]
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string CategoryName { get; set; } = null!;

    public virtual ICollection<EventEntity> Events { get; set; } = [];
}
