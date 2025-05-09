using EventsWebApi.ApiModels.Requests;
using EventsWebApi.ApiModels.Responses;
using EventsWebApi.Handler;
using EventsWebApi.Mapper;
using EventsWebApi.Repositories;


namespace EventsWebApi.Services
{
    public interface IEventService
    {
        Task<EventCreatedResponse?> CreateEventAsync(CreateEventRequest requestData);
        Task<bool> DeleteEventAsync(Guid id);
        Task<IEnumerable<EventResponse>> GetAllEventsAsync();
        Task<IEnumerable<EventResponse>> GetAllEventsByCategoryIdAsync(Guid id);
        Task<EventResponse?> GetEventByIdAsync(Guid id);
        Task<EventResponse?> UpdateEventAsync(Guid id, UpdateEventRequest requestData);
    }

    public class EventService(IEventRepository eventRepository, ICacheHandler<IEnumerable<EventResponse>> cacheHandler) : IEventService
    {
        private readonly IEventRepository _eventRepository = eventRepository;
        private readonly ICacheHandler<IEnumerable<EventResponse>> _cacheHandler = cacheHandler;
        private const string _cacheKey = "Events";

        public async Task<EventCreatedResponse?> CreateEventAsync(CreateEventRequest requestData)
        {
            try
            {
                var entity = EntityMapper.MapToEventEntity(requestData);
                var result = await _eventRepository.AddAsync(entity);
                if (!result)
                    return null;

                _cacheHandler.RemoveCache(_cacheKey);
                return ResponseMapper.MapToEventCreatedResponse(entity);
            }
            catch (Exception) // Fångar alla undantag
            {
                return null; // Signalera misslyckande
            }
        }

        public async Task<bool> DeleteEventAsync(Guid id)
        {
            try
            {
                var success = await _eventRepository.DeleteAsync(x => x.Id == id);

                if (success)
                    _cacheHandler.RemoveCache(_cacheKey);

                return success;
            }
            catch (Exception)
            {
                return false; // Signalera misslyckande
            }
        }

        public async Task<IEnumerable<EventResponse>> GetAllEventsAsync()
        {
            try
            {
                var events = await _cacheHandler.GetOrCreateAsync(_cacheKey, async () =>
                {
                    var entities = await _eventRepository.GetAllAsync();
                    return entities
                        .Select(ResponseMapper.MapToEventResponse)
                        .OrderByDescending(x => x.EventStartDate)
                        .ToList();
                });
                return events ?? Enumerable.Empty<EventResponse>();
            }
            catch (Exception)
            {
                return Enumerable.Empty<EventResponse>(); // Signalera misslyckande
            }
        }

        public async Task<IEnumerable<EventResponse>> GetAllEventsByCategoryIdAsync(Guid categoryId)
        {
            try
            {
                var cachedEvents = _cacheHandler.GetFromCache(_cacheKey);
                if (cachedEvents != null)
                    return cachedEvents.Where(x => x.Category.Id == categoryId);

                var entities = await _eventRepository.GetEventsByCategoryIdAsync(categoryId);
                return entities.Select(ResponseMapper.MapToEventResponse);
            }
            catch (Exception)
            {
                return Enumerable.Empty<EventResponse>(); // Signalera misslyckande
            }
        }

        public async Task<EventResponse?> GetEventByIdAsync(Guid id)
        {
            try
            {
                var cachedEvents = _cacheHandler.GetFromCache(_cacheKey);
                if (cachedEvents != null)
                {
                    var eventFromCache = cachedEvents.FirstOrDefault(x => x.Id == id);
                    if (eventFromCache != null)
                        return eventFromCache;
                }
                var entity = await _eventRepository.GetByIdAsync(x => x.Id == id);
                if (entity == null)
                    return null;

                return ResponseMapper.MapToEventResponse(entity);
            }
            catch (Exception)
            {
                return null; // Signalera misslyckande
            }
        }

        public async Task<EventResponse?> UpdateEventAsync(Guid id, UpdateEventRequest requestData)
        {
            try
            {
                var entity = await _eventRepository.GetByIdAsync(x => x.Id == id);
                if (entity == null)
                    return null;

                if (requestData.Id != Guid.Empty && id != requestData.Id)
                    throw new ArgumentException("ID mismatch between route and body");

                UpdateMapper.UpdateEventEntity(requestData, entity);
                var success = await _eventRepository.UpdateAsync(entity);
                if (!success)
                    return null;

                _cacheHandler.RemoveCache(_cacheKey);
                return ResponseMapper.MapToEventResponse(entity);
            }
            catch (ArgumentException) // Fånga specifikt ArgumentException
            {
                throw; // Återkasta för att låta controllern hantera det
            }
            catch (Exception) // Fånga andra oväntade undantag
            {
                return null; // Signalera misslyckande
            }
        }
    }
}
