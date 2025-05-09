using EventsWebApi.ApiModels.Requests;
using EventsWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EventsWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategoryService categoryService) : ControllerBase
{
    private readonly ICategoryService _categoryService = categoryService;

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryRequest requestData)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _categoryService.CreateCategoryAsync(requestData);

        if (result == null)
            return BadRequest("Failed to create category");

        return Ok(result);
    }


    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var result = await _categoryService.GetAllCategoriesAsync();
        if (result == null)
            return Ok(result);

        return Ok(result);
    }
}