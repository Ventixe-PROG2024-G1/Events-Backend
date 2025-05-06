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
    Task<CategoryResponse?> GetCategoryByIdAsync(Guid Id);
    Task<CategoryResponse?> UpdateCategoryAsync(UpdateCategoryRequest requestData);
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

        _cacheHandler.RemoveCache(_cacheKey);
        return ApiMapper.MapToCategoryResponse(entity);
    }

    public async Task<bool> DeleteCategoryAsync(Guid Id)
    {
        var success = await _categoryRepository.DeleteAsync(x => x.Id == Id);
        if (success)
            _cacheHandler.RemoveCache(_cacheKey);

        return success;
    }

    public async Task<CategoryResponse?> GetCategoryByIdAsync(Guid Id)
    {
        var cachedCategory = _cacheHandler.GetFromCache(_cacheKey);
        if (cachedCategory != null)
        {
            var category = cachedCategory.FirstOrDefault(x => x.Id == Id);
            return category;
        }

        var entity = await _categoryRepository.GetByIdAsync(x => x.Id == Id);
        if (entity == null)
            return null;

        return ApiMapper.MapToCategoryResponse(entity);
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync()
    {
        var categories = await _cacheHandler.GetOrCreateAsync(_cacheKey, async () =>
        {
            var entities = await _categoryRepository.GetAllAsync();

            return entities
                .Select(ApiMapper.MapToCategoryResponse)
                .OrderBy(category => category.CategoryName)
                .ToList();
        });
        return categories ?? Enumerable.Empty<CategoryResponse>();
    }

    public async Task<CategoryResponse?> UpdateCategoryAsync(UpdateCategoryRequest requestData)
    {
        var entity = await _categoryRepository.GetByIdAsync(x => x.Id == requestData.Id);
        if (entity == null)
            return null;

        ApiMapper.UpdateCategoryEntity(requestData, entity);
        var success = await _categoryRepository.UpdateAsync(entity);
        if (!success)
            return null;
        _cacheHandler.RemoveCache(_cacheKey);
        return ApiMapper.MapToCategoryResponse(entity);
    }
}
