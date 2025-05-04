using System.ComponentModel.DataAnnotations;

namespace EventsWebApi.ApiModels.Requests;

public class CreateCategoryRequest
{
    [Required]
    public string CategoryName { get; set; } = null!;
}
