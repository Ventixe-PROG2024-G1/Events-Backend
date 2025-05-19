using EventsWebApi.ApiModels.Requests;
using EventsWebApi.ApiModels.Responses;
using EventsWebApi.Data.Entities;
using EventsWebApi.Handler;
using EventsWebApi.Mapper;
using EventsWebApi.Repositories;
using Microsoft.EntityFrameworkCore;


namespace EventsWebApi.Services
{
    public interface IEventService
    {
        Task<EventCreatedResponse?> CreateEventAsync(CreateEventRequest requestData);
        Task<bool> DeleteEventAsync(Guid id);
        Task<IEnumerable<EventResponse>> GetAllEventsAsync();
        Task<EventResponse?> GetEventByIdAsync(Guid id);
        Task<PagingEventResult> GetEventsPaginatedAsync(int pageNumber, int pageSize, string? categoryNameFilter, string? searchTerm, string? dateFilter, DateTime? specificDateFrom, DateTime? specificDateTo);
        Task<EventResponse?> UpdateEventAsync(Guid id, UpdateEventRequest requestData);
    }

    public class EventService(IEventRepository eventRepository, ICacheHandler<EventResponse?> cacheHandler, ICacheHandler<IEnumerable<EventResponse>> cacheHandlerList) : IEventService
    {
        private readonly IEventRepository _eventRepository = eventRepository;
        private readonly ICacheHandler<EventResponse?> _cacheHandler = cacheHandler;
        private readonly ICacheHandler<IEnumerable<EventResponse>> _cacheHandlerList = cacheHandlerList;
        private const string _cacheKeyList = "EventsList";

        public async Task<EventCreatedResponse?> CreateEventAsync(CreateEventRequest requestData)
        {
            try
            {
                var entity = EntityMapper.MapToEventEntity(requestData);
                var result = await _eventRepository.AddAsync(entity);
                if (!result)
                    return null;

                var createdEntity = await _eventRepository.GetByIdAsync(x => x.Id == entity.Id);
                if (createdEntity == null)
                {
                    return null;
                }

                var eventResponseCached = ResponseMapper.MapToEventResponse(createdEntity);
                if (eventResponseCached != null)
                    _cacheHandler.SetCache(createdEntity.Id.ToString(), eventResponseCached);


                _cacheHandlerList.RemoveCache(_cacheKeyList); // Ta bort cache för listan
                return ResponseMapper.MapToEventCreatedResponse(createdEntity);


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
                    _cacheHandler.RemoveCache(id.ToString());

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
                var cachedEvents =  await _cacheHandlerList.GetOrCreateAsync(_cacheKeyList, async () =>
                {
                    var eventEntities = await _eventRepository.GetAllAsync();
                    if (eventEntities == null || !eventEntities.Any())
                        return Enumerable.Empty<EventResponse>(); // Ingen data hittades

                    return ResponseMapper.MapToEventResponseList(eventEntities)
                    .OrderBy(e => e.EventStartDate)
                    .ToList();
                });

                return cachedEvents ?? Enumerable.Empty<EventResponse>(); // Ingen data hittades
            }
            catch (Exception)
            {
                return Enumerable.Empty<EventResponse>(); // Signalera misslyckande
            }
        }

        public async Task<PagingEventResult> GetEventsPaginatedAsync(int pageNumber, int pageSize, string? categoryNameFilter, string? searchTerm, string? dateFilter, DateTime? specificDateFrom, DateTime? specificDateTo)
        {
            IQueryable<EventEntity> query = _eventRepository.GetQueryable();

            if (!string.IsNullOrEmpty(categoryNameFilter))
                query = query.Where(x => x.Category != null && x.Category.CategoryName.ToLower() == categoryNameFilter.ToLower());


            if (!string.IsNullOrWhiteSpace(dateFilter))
            {
                DateTime now = DateTime.UtcNow.Date;
                DateTime startDateRange = now;
                DateTime endDateRange;

                switch (dateFilter.ToLowerInvariant())
                {
                    case "thisweek":
                        endDateRange = now.AddDays(7);
                        query = query.Where(e => e.EventStartDate >= startDateRange && e.EventStartDate < endDateRange);
                        break;
                    case "thismonth":
                        endDateRange = now.AddDays(30);
                        query = query.Where(e => e.EventStartDate >= startDateRange && e.EventStartDate < endDateRange);
                        break;
                    case "thisyear":
                        endDateRange = now.AddDays(365);
                        query = query.Where(e => e.EventStartDate >= startDateRange && e.EventStartDate < endDateRange);
                        break;
                    case "upcoming":
                        query = query.Where(e => e.EventStartDate >= now);
                        break;
                    case "past":
                        query = query.Where(e => e.EventStartDate < now);
                        break;
                }
            }
            else
            {
                if (specificDateFrom.HasValue)
                {
                    query = query.Where(e => e.EventStartDate >= specificDateFrom.Value.Date);
                }
                if (specificDateTo.HasValue)
                {
                    query = query.Where(e => e.EventStartDate < specificDateTo.Value.Date.AddDays(1));
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(e => (e.EventName != null && e.EventName.ToLower().Contains(lowerSearchTerm)) ||
                                         (e.Description != null && e.Description.ToLower().Contains(lowerSearchTerm)) ||
                                         (e.Category != null && e.Category.CategoryName != null && e.Category.CategoryName.ToLower().Contains(lowerSearchTerm)));
            }
            query = query.OrderBy(e => e.EventStartDate);

            var totalCount = await query.CountAsync();

            var eventEntities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            List<EventResponse> eventResponses = ResponseMapper
                .MapToEventResponseList(eventEntities)
                .ToList();

            // Är denna nödvändig? Kan den orsaka problem?
            foreach (var eventResponse in eventResponses)
            {
                _cacheHandler.SetCache(eventResponse.Id.ToString(), eventResponse);
            }

            return new PagingEventResult(eventResponses, pageNumber, pageSize, totalCount);
        }

        public async Task<EventResponse?> GetEventByIdAsync(Guid id)
        {
            try
            {
                return await _cacheHandler.GetOrCreateAsync(id.ToString(), async () =>
                {
                    var entity = await _eventRepository.GetByIdAsync(x => x.Id == id);
                    return entity != null ? ResponseMapper.MapToEventResponse(entity) : null;
                });
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

                _cacheHandler.RemoveCache(id.ToString());
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
