using EventsWebApi.ApiModels.Requests;
using EventsWebApi.ApiModels.Responses;
using EventsWebApi.Handler;
using EventsWebApi.Mapper;
using EventsWebApi.Repositories;

namespace EventsWebApi.Services;

public interface ICategoryService
{
    Task<CategoryResponse?> CreateCategoryAsync(CreateCategoryRequest requestData);
    Task<bool> DeleteCategoryAsync(Guid Id);
    Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync();
    Task<IEnumerable<CategoryResponse>> UpdateCacheAsync();
}

// Glöm inte Try catcha senare
public class CategoryService(ICategoryRepository categoryRepository, ICacheHandler<IEnumerable<CategoryResponse>> cacheHandler) : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly ICacheHandler<IEnumerable<CategoryResponse>> _cacheHandler = cacheHandler;
    private const string _cacheKey = "Categories";


    public async Task<CategoryResponse?> CreateCategoryAsync(CreateCategoryRequest requestData)
    {
        var entity = ApiMapper.MapToCategoryEntity(requestData);
        var result = await _categoryRepository.AddAsync(entity);

        if (!result)
            return null;

        var response = await UpdateCacheAsync();
        return response.FirstOrDefault(x => x.Id == entity.Id);
    }

    public async Task<bool> DeleteCategoryAsync(Guid Id)
    {
        var entity = await _categoryRepository.DeleteAsync(x => x.Id == Id);
        if (entity)
            await UpdateCacheAsync();

        return entity;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync() =>
        _cacheHandler.GetFromCache(_cacheKey) ?? await UpdateCacheAsync();

    public async Task<IEnumerable<CategoryResponse>> UpdateCacheAsync()
    {
        var entities = await _categoryRepository.GetAllAsync();
        var responses = entities
            .Select(ApiMapper.MapToCategoryResponse)
            .OrderBy(category => category.CategoryName)
            .ToList();

        _cacheHandler.SetCache(_cacheKey, responses);
        return responses;
    }
}
