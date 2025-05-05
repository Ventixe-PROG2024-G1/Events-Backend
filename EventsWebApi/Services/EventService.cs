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
        if (!result)
            return null;

        _cacheHandler.RemoveCache(_cacheKey);
        return ApiMapper.MapToEventResponse(entity);
    }

    public async Task<bool> DeleteEventAsync(Guid Id)
    {
        var success = await _eventRepository.DeleteAsync(x => x.Id == Id);

        if (success)
            _cacheHandler.RemoveCache(_cacheKey);

        return success;
    }

    public async Task<IEnumerable<EventResponse>> GetAllEventsAsync()
    {
        var events = await _cacheHandler.GetOrCreateAsync(_cacheKey, async () =>
        {
            var entities = await _eventRepository.GetAllAsync();
            return entities
                .Select(ApiMapper.MapToEventResponse)
                .OrderByDescending(x => x.EventStartDate)
                .ToList();
        });
        return events ?? Enumerable.Empty<EventResponse>();
    }


    public async Task<IEnumerable<EventResponse>> GetAllEventsByCategoryIdAsync(Guid Id)
    {
        var cachedEvents = _cacheHandler.GetFromCache(_cacheKey);
        if (cachedEvents != null)
            return cachedEvents.Where(x => x.Category.Id == Id);

        var entities = await _eventRepository.GetEventsByCategoryIdAsync(Id);

        return entities.Select(ApiMapper.MapToEventResponse);
    }

    public async Task<EventResponse?> GetEventByIdAsync(Guid Id)
    {
        var cachedEvents = _cacheHandler.GetFromCache(_cacheKey);
        if (cachedEvents != null)
        {
            var eventFromCache = cachedEvents.FirstOrDefault(x => x.Id == Id);
            return eventFromCache;
        }
        var entity = await _eventRepository.GetByIdAsync(x => x.Id == Id);
        if (entity == null)
            return null;

        return ApiMapper.MapToEventResponse(entity);
    }

    public async Task<EventResponse?> UpdateEventAsync(UpdateEventRequest requestData)
    {
        var entity = await _eventRepository.GetByIdAsync(x => x.Id == requestData.Id);
        if (entity == null) 
            return null;

        ApiMapper.UpdateEventEntity(requestData, entity);
        var success = await _eventRepository.UpdateAsync(entity);
        if (!success)
            return null;
        _cacheHandler.RemoveCache(_cacheKey);
        return ApiMapper.MapToEventResponse(entity);
    }
}
