using EventsWebApi.ApiModels.Requests;
using EventsWebApi.ApiModels.Responses;
using EventsWebApi.Handler;
using EventsWebApi.Mapper;
using EventsWebApi.Repositories;

namespace EventsWebApi.Services;

public interface IEventService
{
    Task<EventResponse?> CreateEventAsync(CreateEventRequest requestData);
    Task<bool> DeleteEventAsync(Guid Id);
    Task<IEnumerable<EventResponse>> GetAllEventsAsync();
    Task<IEnumerable<EventResponse>> GetAllEventsByCategoryIdAsync(Guid categoryId);
    Task<EventResponse?> GetEventByIdAsync(Guid Id);
    Task<IEnumerable<EventResponse>> UpdateCacheAsync();
    Task<EventResponse?> UpdateEventAsync(UpdateEventRequest requestData);
}

public class EventService(IEventRepository eventRepository, ICacheHandler<IEnumerable<EventResponse>> cacheHandler) : IEventService
{
    private readonly IEventRepository _eventRepository = eventRepository;
    private readonly ICacheHandler<IEnumerable<EventResponse>> _cacheHandler = cacheHandler;
    private const string _cacheKey = "Events";

    // Glöm inte Try catcha senare

    public async Task<EventResponse?> CreateEventAsync(CreateEventRequest requestData)
    {
        var entity = ApiMapper.MapToEventEntity(requestData);
        var result = await _eventRepository.AddAsync(entity);

        var response = await UpdateCacheAsync();
        return response.FirstOrDefault(x => x.Id == entity.Id);
    }

    public async Task<bool> DeleteEventAsync(Guid Id)
    {
        var entity = await _eventRepository.DeleteAsync(x => x.Id == Id);

        if (entity)
            await UpdateCacheAsync();

        return entity;
    }

    public async Task<IEnumerable<EventResponse>> GetAllEventsAsync() =>
        _cacheHandler.GetFromCache(_cacheKey) ?? await UpdateCacheAsync();


    public async Task<IEnumerable<EventResponse>> GetAllEventsByCategoryIdAsync(Guid categoryId)
    {
        var entity = _cacheHandler.GetFromCache(_cacheKey)?
            .Where(x => x.Category.Id == categoryId);

        if (entity != null)
            return entity;

        var categoryEvents = await _eventRepository.GetEventsByCategoryIdAsync(categoryId);
        var response = categoryEvents
            .Select(ApiMapper.MapToEventResponse)
            .OrderByDescending(x => x.EventStartDate)
            .ToList();

        return response;
    }

    public async Task<EventResponse?> GetEventByIdAsync(Guid Id)
    {
        var entity = _cacheHandler.GetFromCache(_cacheKey)?
            .FirstOrDefault(x => x.Id == Id);

        if (entity != null)
            return entity;

        var response = await UpdateCacheAsync();
        return response.FirstOrDefault(x => x.Id == Id);
    }

    public async Task<EventResponse?> UpdateEventAsync(UpdateEventRequest requestData)
    {
        var entity = await _eventRepository.GetByIdAsync(x => x.Id == requestData.Id);
        if (entity == null)
            return null;

        ApiMapper.UpdateEventEntity(requestData, entity);
        await _eventRepository.UpdateAsync(entity);

        var response = await UpdateCacheAsync();
        return response.FirstOrDefault(x => x.Id == entity.Id);
    }

    public async Task<IEnumerable<EventResponse>> UpdateCacheAsync()
    {
        var entities = await _eventRepository.GetAllAsync();
        var responses = entities
            .Select(ApiMapper.MapToEventResponse)
            .OrderByDescending(x => x.EventStartDate)
            .ToList();

        _cacheHandler.SetCache(_cacheKey, responses);
        return responses;
    }
}
