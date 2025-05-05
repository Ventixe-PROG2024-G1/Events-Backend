using System.ComponentModel.DataAnnotations;

namespace EventsWebApi.ApiModels.Requests;

public class UpdateCategoryRequest
{
    public Guid Id { get; set; }
    public string? CategoryName { get; set; }
}
