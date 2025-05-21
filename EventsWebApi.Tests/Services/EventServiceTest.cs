using EventsWebApi.ApiModels.Requests;
using EventsWebApi.ApiModels.Responses;
using EventsWebApi.Data.Entities;
using EventsWebApi.Domain;
using EventsWebApi.Handler;
using EventsWebApi.Mapper;
using EventsWebApi.Repositories;
using EventsWebApi.Services;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;
using MockQueryable;

namespace EventsWebApi.Tests.Services;

public class EventServiceTest
{
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly Mock<ICacheHandler<EventResponse?>> _mockCacheHandler;
    private readonly Mock<ICacheHandler<IEnumerable<EventResponse>>> _mockCacheHandlerList;
    private readonly EventService _eventService;

    public EventServiceTest()
    {
        _mockEventRepository = new Mock<IEventRepository>();
        _mockCacheHandler = new Mock<ICacheHandler<EventResponse?>>();
        _mockCacheHandlerList = new Mock<ICacheHandler<IEnumerable<EventResponse>>>();
        _eventService = new EventService(
            _mockEventRepository.Object,
            _mockCacheHandler.Object,
            _mockCacheHandlerList.Object);
    }

    [Fact]
    public async Task CreateEventAsync_ValidRequest_ReturnsEventCreatedResponseAndCaches()
    {
        // Arrange
        var testEventId = Guid.NewGuid(); // Använd ett konsekvent ID genom hela testet
        var testCategoryId = Guid.NewGuid();

        var request = new CreateEventRequest
        {
            EventName = "Test Event",
            CategoryId = testCategoryId,
            EventStartDate = DateTime.UtcNow,
            EventEndDate = DateTime.UtcNow.AddHours(2),
            Status = EventStatus.Active
        };

        // Detta är entiteten som vi FÖRVÄNTAR OSS att GetByIdAsync ska returnera
        // Den ska ha ID satt och Category populerad, precis som den riktiga GetByIdAsync skulle göra.
        var fullyPopulatedEntityFromDbMock = new EventEntity
        {
            Id = testEventId, // Samma ID som AddAsync-mocken kommer att "sätta"
            EventName = request.EventName,
            Description = request.Description, // Se till att alla relevanta fält finns med
            CategoryId = testCategoryId,
            Category = new CategoryEntity { Id = testCategoryId, CategoryName = "Mocked Category" },
            EventStartDate = request.EventStartDate,
            EventEndDate = request.EventEndDate,
            Status = request.Status
            // ... andra fält från request ...
        };

        _mockEventRepository.Setup(repo => repo.AddAsync(It.IsAny<EventEntity>()))
            .ReturnsAsync((EventEntity entityPassedToService) => {
                entityPassedToService.Id = testEventId;
                return true;
            });

        _mockEventRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Expression<Func<EventEntity, bool>>>())) // Matcha vilket predikat som helst först
            .ReturnsAsync((Expression<Func<EventEntity, bool>> predicate) =>
            {
                var entityToTestPredicateWith = new EventEntity { Id = testEventId };

                if (predicate.Compile()(entityToTestPredicateWith)) 
                {

                    return fullyPopulatedEntityFromDbMock;
                }
                return null;
            });

        var expectedResponse = ResponseMapper.MapToEventCreatedResponse(fullyPopulatedEntityFromDbMock);
        var expectedCachedResponse = ResponseMapper.MapToEventResponse(fullyPopulatedEntityFromDbMock);


        // Act
        var result = await _eventService.CreateEventAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);
        Assert.Equal(expectedResponse.EventName, result.EventName);
        _mockCacheHandler.Verify(cache => cache.SetCache(
            testEventId.ToString(),
            It.Is<EventResponse>(er => er.Id == expectedCachedResponse.Id && er.EventName == expectedCachedResponse.EventName), // Mer specifik matchning av objektet
            It.IsAny<int>()),
            Times.Once);
        _mockCacheHandlerList.Verify(cache => cache.RemoveCache("EventsList"), Times.Once);
    }

    [Fact]
    public async Task CreateEventAsync_RepositoryAddFails_ReturnsNull()
    {
        // Arrange
        var request = new CreateEventRequest();
        _mockEventRepository.Setup(repo => repo.AddAsync(It.IsAny<EventEntity>())).ReturnsAsync(false);

        // Act
        var result = await _eventService.CreateEventAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateEventAsync_GetByIdAfterAddFails_ReturnsNull()
    {
        // Arrange
        var request = new CreateEventRequest
        {
            EventName = "Test Event",
            CategoryId = Guid.NewGuid(),
            EventStartDate = DateTime.UtcNow,
            EventEndDate = DateTime.UtcNow.AddHours(2),
            Status = EventStatus.Active
        };
        _mockEventRepository.Setup(repo => repo.AddAsync(It.IsAny<EventEntity>())).ReturnsAsync(true);
        _mockEventRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Expression<Func<EventEntity, bool>>>()))
            .ReturnsAsync((EventEntity?)null);

        // Act
        var result = await _eventService.CreateEventAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateEventAsync_ExceptionOccurs_ReturnsNull()
    {
        // Arrange
        var request = new CreateEventRequest();
        _mockEventRepository.Setup(repo => repo.AddAsync(It.IsAny<EventEntity>())).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _eventService.CreateEventAsync(request);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public async Task DeleteEventAsync_EventExists_ReturnsTrueAndRemovesFromCache()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mockEventRepository.Setup(repo => repo.DeleteAsync(It.IsAny<Expression<Func<EventEntity, bool>>>())).ReturnsAsync(true);

        // Act
        var result = await _eventService.DeleteEventAsync(eventId);

        // Assert
        Assert.True(result);
        _mockCacheHandler.Verify(cache => cache.RemoveCache(eventId.ToString()), Times.Once);
    }

    [Fact]
    public async Task DeleteEventAsync_EventNotFound_ReturnsFalse()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mockEventRepository.Setup(repo => repo.DeleteAsync(It.IsAny<Expression<Func<EventEntity, bool>>>())).ReturnsAsync(false);

        // Act
        var result = await _eventService.DeleteEventAsync(eventId);

        // Assert
        Assert.False(result);
        _mockCacheHandler.Verify(cache => cache.RemoveCache(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteEventAsync_ExceptionOccurs_ReturnsFalse()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mockEventRepository.Setup(repo => repo.DeleteAsync(It.IsAny<Expression<Func<EventEntity, bool>>>())).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _eventService.DeleteEventAsync(eventId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllEventsAsync_EventsExist_ReturnsEventList()
    {
        // Arrange
        var eventEntities = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), EventName = "Event 1", EventStartDate = DateTime.UtcNow, Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat1" } },
            new() { Id = Guid.NewGuid(), EventName = "Event 2", EventStartDate = DateTime.UtcNow.AddDays(1), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat2" } }
        };
        var expectedResponses = ResponseMapper.MapToEventResponseList(eventEntities).OrderBy(e => e.EventStartDate).ToList();

        _mockCacheHandlerList.Setup(cache => cache.GetOrCreateAsync("EventsList", It.IsAny<Func<Task<IEnumerable<EventResponse>?>>>(), It.IsAny<int>()))
            .ReturnsAsync(expectedResponses);

        // Act
        var result = await _eventService.GetAllEventsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponses.Count(), result.Count());
        Assert.Equal(expectedResponses.First().EventName, result.First().EventName);
    }

    [Fact]
    public async Task GetAllEventsAsync_NoEvents_ReturnsEmptyList()
    {
        // Arrange
        _mockCacheHandlerList.Setup(cache => cache.GetOrCreateAsync("EventsList", It.IsAny<Func<Task<IEnumerable<EventResponse>?>>>(), It.IsAny<int>()))
            .ReturnsAsync(Enumerable.Empty<EventResponse>());
        // Act
        var result = await _eventService.GetAllEventsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllEventsAsync_CacheMiss_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        var eventEntities = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), EventName = "Event 1", EventStartDate = DateTime.UtcNow, Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat1" } },
            new() { Id = Guid.NewGuid(), EventName = "Event 2", EventStartDate = DateTime.UtcNow.AddDays(1), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat2" } }
        };
        var expectedResponses = ResponseMapper.MapToEventResponseList(eventEntities).OrderBy(e => e.EventStartDate).ToList();

        _mockCacheHandlerList.Setup(cache => cache.GetOrCreateAsync("EventsList", It.IsAny<Func<Task<IEnumerable<EventResponse>?>>>(), It.IsAny<int>()))
            .Returns(async (string key, Func<Task<IEnumerable<EventResponse>?>> factory, int minutes) => await factory());

        _mockEventRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(eventEntities);

        // Act
        var result = await _eventService.GetAllEventsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponses.Count(), result.Count());
        _mockEventRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllEventsAsync_ExceptionOccurs_ReturnsEmptyList()
    {
        // Arrange
        _mockCacheHandlerList.Setup(cache => cache.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<IEnumerable<EventResponse>?>>>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Cache or DB error"));

        // Act
        var result = await _eventService.GetAllEventsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEventByIdAsync_EventExists_ReturnsEventResponse()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventEntity = new EventEntity { Id = eventId, EventName = "Test Event", Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Test" } };
        var expectedResponse = ResponseMapper.MapToEventResponse(eventEntity);

        _mockCacheHandler.Setup(cache => cache.GetOrCreateAsync(eventId.ToString(), It.IsAny<Func<Task<EventResponse?>>>(), It.IsAny<int>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _eventService.GetEventByIdAsync(eventId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);
    }

    [Fact]
    public async Task GetEventByIdAsync_CacheMiss_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventEntity = new EventEntity { Id = eventId, EventName = "Test Event", Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Test" } };
        var expectedResponse = ResponseMapper.MapToEventResponse(eventEntity);

        _mockCacheHandler.Setup(cache => cache.GetOrCreateAsync(eventId.ToString(), It.IsAny<Func<Task<EventResponse?>>>(), It.IsAny<int>()))
            .Returns(async (string key, Func<Task<EventResponse?>> factory, int minutes) =>
            {
                _mockEventRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Expression<Func<EventEntity, bool>>>()))
                    .ReturnsAsync((Expression<Func<EventEntity, bool>> predicate) =>
                    {
                        // Simulate the predicate evaluation
                        if (predicate.Compile().Invoke(eventEntity))
                            return eventEntity;
                        return null;
                    });
                return await factory();
            });


        // Act
        var result = await _eventService.GetEventByIdAsync(eventId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);
        _mockEventRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<Expression<Func<EventEntity, bool>>>()), Times.Once);
    }


    [Fact]
    public async Task GetEventByIdAsync_EventNotFound_ReturnsNull()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mockCacheHandler.Setup(cache => cache.GetOrCreateAsync(eventId.ToString(), It.IsAny<Func<Task<EventResponse?>>>(), It.IsAny<int>()))
            .ReturnsAsync((EventResponse?)null);

        // Act
        var result = await _eventService.GetEventByIdAsync(eventId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetEventByIdAsync_ExceptionOccurs_ReturnsNull()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mockCacheHandler.Setup(cache => cache.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<EventResponse?>>>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Cache or DB error"));

        // Act
        var result = await _eventService.GetEventByIdAsync(eventId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateEventAsync_ValidRequest_EventExists_ReturnsUpdatedEventResponse()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var request = new UpdateEventRequest { Id = eventId, EventName = "Updated Event Name" };
        var existingEntity = new EventEntity { Id = eventId, EventName = "Old Event Name", Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Test" } };

        _mockEventRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Expression<Func<EventEntity, bool>>>())).ReturnsAsync(existingEntity);
        _mockEventRepository.Setup(repo => repo.UpdateAsync(It.IsAny<EventEntity>())).ReturnsAsync(true);

        // Act
        var result = await _eventService.UpdateEventAsync(eventId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.EventName, result.EventName);
        _mockCacheHandler.Verify(cache => cache.RemoveCache(eventId.ToString()), Times.Once);
        _mockEventRepository.Verify(repo => repo.UpdateAsync(It.Is<EventEntity>(e => e.EventName == request.EventName)), Times.Once);
    }

    [Fact]
    public async Task UpdateEventAsync_ValidRequest_EmptyRequestIdInBody_ReturnsUpdatedEventResponse()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var request = new UpdateEventRequest { Id = Guid.Empty, EventName = "Updated Event Name Via Empty Body Id" }; // Id is Empty
        var existingEntity = new EventEntity { Id = eventId, EventName = "Old Name", Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Test" } };

        _mockEventRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Expression<Func<EventEntity, bool>>>()))
                            .ReturnsAsync(existingEntity);
        _mockEventRepository.Setup(repo => repo.UpdateAsync(It.IsAny<EventEntity>())).ReturnsAsync(true);

        // Act
        var result = await _eventService.UpdateEventAsync(eventId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.EventName, result.EventName);
        _mockCacheHandler.Verify(cache => cache.RemoveCache(eventId.ToString()), Times.Once);
    }

    [Fact]
    public async Task UpdateEventAsync_EventNotFound_ReturnsNull()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var request = new UpdateEventRequest { Id = eventId };
        _mockEventRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Expression<Func<EventEntity, bool>>>())).ReturnsAsync((EventEntity?)null);

        // Act
        var result = await _eventService.UpdateEventAsync(eventId, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateEventAsync_IdMismatch_ThrowsArgumentException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var request = new UpdateEventRequest { Id = Guid.NewGuid(), EventName = "Mismatch Event" }; // Different Id in request
        var existingEntity = new EventEntity { Id = eventId, EventName = "Old Event Name" };

        _mockEventRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Expression<Func<EventEntity, bool>>>())).ReturnsAsync(existingEntity);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _eventService.UpdateEventAsync(eventId, request));
    }

    [Fact]
    public async Task UpdateEventAsync_RepositoryUpdateFails_ReturnsNull()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var request = new UpdateEventRequest { Id = eventId, EventName = "Updated Event Name" };
        var existingEntity = new EventEntity { Id = eventId, EventName = "Old Event Name", Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Test" } };

        _mockEventRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Expression<Func<EventEntity, bool>>>())).ReturnsAsync(existingEntity);
        _mockEventRepository.Setup(repo => repo.UpdateAsync(It.IsAny<EventEntity>())).ReturnsAsync(false);

        // Act
        var result = await _eventService.UpdateEventAsync(eventId, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateEventAsync_GeneralExceptionOccurs_ReturnsNull()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var request = new UpdateEventRequest { Id = eventId };
        _mockEventRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Expression<Func<EventEntity, bool>>>())).ThrowsAsync(new System.Net.Sockets.SocketException()); // Simulate a non-ArgumentException

        // Act
        var result = await _eventService.UpdateEventAsync(eventId, request);

        // Assert
        Assert.Null(result);
    }

    // Helper for GetEventsPaginatedAsync
    private IQueryable<EventEntity> GetTestEventEntitiesQueryable(List<EventEntity> entities)
    {
        var asyncEnumerable = new TestAsyncEnumerable<EventEntity>(entities);
        var mockQueryable = new Mock<IQueryable<EventEntity>>();

        mockQueryable.As<IAsyncEnumerable<EventEntity>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(asyncEnumerable.GetAsyncEnumerator());
        mockQueryable.As<IQueryable<EventEntity>>().Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<EventEntity>(entities.AsQueryable().Provider)); // Use the provider of the in-memory list
        mockQueryable.As<IQueryable<EventEntity>>().Setup(m => m.Expression).Returns(entities.AsQueryable().Expression);
        mockQueryable.As<IQueryable<EventEntity>>().Setup(m => m.ElementType).Returns(entities.AsQueryable().ElementType);
        mockQueryable.As<IQueryable<EventEntity>>().Setup(m => m.GetEnumerator()).Returns(() => entities.GetEnumerator());

        return mockQueryable.Object;
    }


    [Fact]
    public async Task GetEventsPaginatedAsync_NoFilters_ReturnsPaginatedResult()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 2;
        var allEntities = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), EventName = "Event 1", EventStartDate = DateTime.UtcNow.AddDays(1), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat1" } },
            new() { Id = Guid.NewGuid(), EventName = "Event 2", EventStartDate = DateTime.UtcNow.AddDays(2), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat2" } },
            new() { Id = Guid.NewGuid(), EventName = "Event 3", EventStartDate = DateTime.UtcNow.AddDays(3), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat3" } }
        };

        // Use MockQueryable to create a mock IQueryable that supports async
        var mockQueryable = allEntities.AsQueryable().BuildMock();

        _mockEventRepository.Setup(repo => repo.GetQueryable()).Returns(mockQueryable);

        // Act
        var result = await _eventService.GetEventsPaginatedAsync(pageNumber, pageSize, null, null, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(allEntities.Count, result.TotalCount);
        Assert.Equal(pageSize, result.Events.Count); // Expecting first 2 items
        Assert.Equal("Event 1", result.Events[0].EventName);
        _mockCacheHandler.Verify(cache => cache.SetCache(It.IsAny<string>(), It.IsAny<EventResponse>(), It.IsAny<int>()), Times.Exactly(result.Events.Count));
    }

    [Fact]
    public async Task GetEventsPaginatedAsync_WithCategoryFilter_ReturnsFilteredResult()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 5;
        string categoryFilter = "FilteredCat";
        var allEntities = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), EventName = "Event 1", EventStartDate = DateTime.UtcNow.AddDays(1), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = categoryFilter } },
            new() { Id = Guid.NewGuid(), EventName = "Event 2", EventStartDate = DateTime.UtcNow.AddDays(2), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "OtherCat" } },
            new() { Id = Guid.NewGuid(), EventName = "Event 3", EventStartDate = DateTime.UtcNow.AddDays(3), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = categoryFilter } }
        };
        var mockQueryable = allEntities.AsQueryable().BuildMock();
        _mockEventRepository.Setup(repo => repo.GetQueryable()).Returns(mockQueryable);

        // Act
        var result = await _eventService.GetEventsPaginatedAsync(pageNumber, pageSize, categoryFilter, null, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount); // Only 2 events match the category
        Assert.Equal(2, result.Events.Count);
        Assert.True(result.Events.All(e => e.Category.CategoryName.Equals(categoryFilter, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task GetEventsPaginatedAsync_WithSearchTerm_ReturnsFilteredResult()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 5;
        string searchTerm = "UniqueName";
        var allEntities = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), EventName = "Event UniqueName One", Description = "Desc1", EventStartDate = DateTime.UtcNow.AddDays(1), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat1" } },
            new() { Id = Guid.NewGuid(), EventName = "Event Two", Description = "Desc UniqueName Two", EventStartDate = DateTime.UtcNow.AddDays(2), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat2" } },
            new() { Id = Guid.NewGuid(), EventName = "Event Three", Description = "Desc3", EventStartDate = DateTime.UtcNow.AddDays(3), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat UniqueName Three" } },
            new() { Id = Guid.NewGuid(), EventName = "Event Four", Description = "Desc4", EventStartDate = DateTime.UtcNow.AddDays(4), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat4" } }
        };
        // Use BuildMock() to enable async operations
        var mockQueryable = allEntities.AsQueryable().BuildMock(); 
        _mockEventRepository.Setup(repo => repo.GetQueryable()).Returns(mockQueryable);

        // Act
        var result = await _eventService.GetEventsPaginatedAsync(pageNumber, pageSize, null, searchTerm, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Events.Count);
        Assert.Contains(result.Events, e => e.EventName.Contains(searchTerm));
        Assert.Contains(result.Events, e => e.Description != null && e.Description.Contains(searchTerm));
        Assert.Contains(result.Events, e => e.Category.CategoryName.Contains(searchTerm));
    }

    [Fact]
    public async Task GetEventsPaginatedAsync_WithDateFilterThisWeek_ReturnsFilteredResult()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 5;
        string dateFilter = "thisweek";
        DateTime now = DateTime.UtcNow.Date;
        var allEntities = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), EventName = "Event Today", EventStartDate = now, Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } },
            new() { Id = Guid.NewGuid(), EventName = "Event In 3 Days", EventStartDate = now.AddDays(3), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } },
            new() { Id = Guid.NewGuid(), EventName = "Event In 8 Days", EventStartDate = now.AddDays(8), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } }, // Outside this week
            new() { Id = Guid.NewGuid(), EventName = "Event Yesterday", EventStartDate = now.AddDays(-1), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } } // Outside this week (past)
        };
        var mockQueryable = allEntities.AsQueryable().BuildMock();
        _mockEventRepository.Setup(repo => repo.GetQueryable()).Returns(mockQueryable);

        // Act
        var result = await _eventService.GetEventsPaginatedAsync(pageNumber, pageSize, null, null, dateFilter, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Events.Count);
        Assert.Contains(result.Events, e => e.EventName == "Event Today");
        Assert.Contains(result.Events, e => e.EventName == "Event In 3 Days");
    }

    [Fact]
    public async Task GetEventsPaginatedAsync_WithSpecificDateRange_ReturnsFilteredResult()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 5;
        DateTime dateFrom = DateTime.UtcNow.Date.AddDays(2);
        DateTime dateTo = DateTime.UtcNow.Date.AddDays(5); // Events on dateTo should be excluded, up to dateTo.AddDays(1)

        var allEntities = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), EventName = "Event Day 1", EventStartDate = DateTime.UtcNow.Date.AddDays(1), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } }, // Before range
            new() { Id = Guid.NewGuid(), EventName = "Event Day 2", EventStartDate = dateFrom, Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } }, // Inside range
            new() { Id = Guid.NewGuid(), EventName = "Event Day 4", EventStartDate = DateTime.UtcNow.Date.AddDays(4), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } }, // Inside range
            new() { Id = Guid.NewGuid(), EventName = "Event Day 5", EventStartDate = dateTo, Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } }, // Inside range (boundary)
            new() { Id = Guid.NewGuid(), EventName = "Event Day 6", EventStartDate = DateTime.UtcNow.Date.AddDays(6), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } }  // After range
        };
        var mockQueryable = allEntities.AsQueryable().BuildMock();
        _mockEventRepository.Setup(repo => repo.GetQueryable()).Returns(mockQueryable);

        // Act
        var result = await _eventService.GetEventsPaginatedAsync(pageNumber, pageSize, null, null, null, dateFrom, dateTo);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Events.Count);
        Assert.Contains(result.Events, e => e.EventName == "Event Day 2");
        Assert.Contains(result.Events, e => e.EventName == "Event Day 4");
        Assert.Contains(result.Events, e => e.EventName == "Event Day 5");
    }

    [Fact]
    public async Task GetEventsPaginatedAsync_WithDateFilterUpcoming_ReturnsFilteredAndOrderedResult()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 5;
        string dateFilter = "upcoming";
        DateTime now = DateTime.UtcNow.Date;
        var allEntities = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), EventName = "Event Tomorrow", EventStartDate = now.AddDays(1), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } },
            new() { Id = Guid.NewGuid(), EventName = "Event Today", EventStartDate = now, Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } },
            new() { Id = Guid.NewGuid(), EventName = "Event Yesterday", EventStartDate = now.AddDays(-1), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } }, // Past
            new() { Id = Guid.NewGuid(), EventName = "Event In 5 Days", EventStartDate = now.AddDays(5), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } }
        }.AsQueryable();

        var mockQueryable = allEntities.AsQueryable().BuildMock();
        _mockEventRepository.Setup(repo => repo.GetQueryable()).Returns(mockQueryable);

        // Act
        var result = await _eventService.GetEventsPaginatedAsync(pageNumber, pageSize, null, null, dateFilter, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount); // Today, Tomorrow, In 5 Days
        Assert.Equal(3, result.Events.Count);
        Assert.Equal("Event Today", result.Events[0].EventName); // Should be ordered by date
        Assert.Equal("Event Tomorrow", result.Events[1].EventName);
        Assert.Equal("Event In 5 Days", result.Events[2].EventName);
        Assert.True(result.Events.All(e => e.EventStartDate >= now));
    }

    [Fact]
    public async Task GetEventsPaginatedAsync_WithDateFilterPast_ReturnsFilteredAndOrderedResult()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 5;
        string dateFilter = "past";
        DateTime now = DateTime.UtcNow.Date;
        var allEntities = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), EventName = "Event Yesterday", EventStartDate = now.AddDays(-1), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } },
            new() { Id = Guid.NewGuid(), EventName = "Event 5 Days Ago", EventStartDate = now.AddDays(-5), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } },
            new() { Id = Guid.NewGuid(), EventName = "Event Today", EventStartDate = now, Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } }, // Should be excluded
            new() { Id = Guid.NewGuid(), EventName = "Event Tomorrow", EventStartDate = now.AddDays(1), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } } // Should be excluded
        }.AsQueryable();

        var mockQueryable = allEntities.AsQueryable().BuildMock();
        _mockEventRepository.Setup(repo => repo.GetQueryable()).Returns(mockQueryable);

        // Act
        var result = await _eventService.GetEventsPaginatedAsync(pageNumber, pageSize, null, null, dateFilter, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Events.Count);
        Assert.Equal("Event Yesterday", result.Events[0].EventName);
        Assert.Equal("Event 5 Days Ago", result.Events[1].EventName);
        Assert.True(result.Events.All(e => e.EventStartDate < now));
    }
}

// Helper classes for testing IQueryable async operations
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];

        var executeMethod = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(Expression) }
            );

        if (executeMethod == null)
        {
            throw new InvalidOperationException($"Could not find generic Execute method on {nameof(IQueryProvider)}.");
        }
        var genericExecuteMethod = executeMethod.MakeGenericMethod(expectedResultType);
        var executionResult = genericExecuteMethod.Invoke(this, new[] { expression });

        if (executionResult == null)
        {
            throw new InvalidOperationException("Execution result is null.");
        }

        var fromResultMethod = typeof(Task).GetMethod(nameof(Task.FromResult));
        if (fromResultMethod == null)
        {
            throw new InvalidOperationException($"Could not find method {nameof(Task.FromResult)} on {nameof(Task)}.");
        }
        var genericFromResultMethod = fromResultMethod.MakeGenericMethod(expectedResultType);
        var taskResult = genericFromResultMethod.Invoke(null, new[] { executionResult });

        if (taskResult == null)
        {
            throw new InvalidOperationException("Task result is null.");
        }

        return (TResult)taskResult;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public void Dispose() // Not async
    {
        _inner.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public T Current => _inner.Current;
}