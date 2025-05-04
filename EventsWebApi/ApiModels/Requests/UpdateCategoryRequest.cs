using System.ComponentModel.DataAnnotations;

namespace EventsWebApi.ApiModels.Requests;

public class UpdateCategoryRequest
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string? CategoryName { get; set; }
}
