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

// Majoriteten är genererad av Copilots Unit Test Feature.
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
        var testEventId = Guid.NewGuid();
        var testCategoryId = Guid.NewGuid();

        var request = new CreateEventRequest
        {
            EventName = "Test Event",
            CategoryId = testCategoryId,
            EventStartDate = DateTime.UtcNow,
            EventEndDate = DateTime.UtcNow.AddHours(2),
            Status = EventStatus.Active
        };

        var fullyPopulatedEntityFromDbMock = new EventEntity
        {
            Id = testEventId,
            EventName = request.EventName,
            Description = request.Description,
            CategoryId = testCategoryId,
            Category = new CategoryEntity { Id = testCategoryId, CategoryName = "Mocked Category" },
            EventStartDate = request.EventStartDate,
            EventEndDate = request.EventEndDate,
            Status = request.Status
        };

        _mockEventRepository.Setup(repo => repo.AddAsync(It.IsAny<EventEntity>()))
            .ReturnsAsync((EventEntity entityPassedToService) => {
                entityPassedToService.Id = testEventId;
                return true;
            });

        _mockEventRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Expression<Func<EventEntity, bool>>>()))
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
            It.Is<EventResponse>(er => er.Id == expectedCachedResponse.Id && er.EventName == expectedCachedResponse.EventName),
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
        var request = new UpdateEventRequest { Id = Guid.Empty, EventName = "Updated Event Name Via Empty Body Id" };
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
        var request = new UpdateEventRequest { Id = Guid.NewGuid(), EventName = "Mismatch Event" };
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
        _mockEventRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Expression<Func<EventEntity, bool>>>())).ThrowsAsync(new System.Net.Sockets.SocketException());

        // Act
        var result = await _eventService.UpdateEventAsync(eventId, request);

        // Assert
        Assert.Null(result);
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

        var mockQueryable = allEntities.AsQueryable().BuildMock();

        _mockEventRepository.Setup(repo => repo.GetQueryable()).Returns(mockQueryable);

        // Act
        var result = await _eventService.GetEventsPaginatedAsync(pageNumber, pageSize, null, null, null, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(allEntities.Count, result.TotalCount);
        Assert.Equal(pageSize, result.Events.Count);
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
        var result = await _eventService.GetEventsPaginatedAsync(pageNumber, pageSize, categoryFilter, null, null, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
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
        var mockQueryable = allEntities.AsQueryable().BuildMock(); 
        _mockEventRepository.Setup(repo => repo.GetQueryable()).Returns(mockQueryable);

        // Act
        var result = await _eventService.GetEventsPaginatedAsync(pageNumber, pageSize, null, searchTerm, null, null, null, null);

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
            new() { Id = Guid.NewGuid(), EventName = "Event In 8 Days", EventStartDate = now.AddDays(8), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } },
            new() { Id = Guid.NewGuid(), EventName = "Event Yesterday", EventStartDate = now.AddDays(-1), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } }
        };
        var mockQueryable = allEntities.AsQueryable().BuildMock();
        _mockEventRepository.Setup(repo => repo.GetQueryable()).Returns(mockQueryable);

        // Act
        var result = await _eventService.GetEventsPaginatedAsync(pageNumber, pageSize, null, null, dateFilter, null, null, null);

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
        DateTime dateTo = DateTime.UtcNow.Date.AddDays(5);

        var allEntities = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), EventName = "Event Day 1", EventStartDate = DateTime.UtcNow.Date.AddDays(1), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } },
            new() { Id = Guid.NewGuid(), EventName = "Event Day 2", EventStartDate = dateFrom, Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } },
            new() { Id = Guid.NewGuid(), EventName = "Event Day 4", EventStartDate = DateTime.UtcNow.Date.AddDays(4), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } },
            new() { Id = Guid.NewGuid(), EventName = "Event Day 5", EventStartDate = dateTo, Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } },
            new() { Id = Guid.NewGuid(), EventName = "Event Day 6", EventStartDate = DateTime.UtcNow.Date.AddDays(6), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } }
        };
        var mockQueryable = allEntities.AsQueryable().BuildMock();
        _mockEventRepository.Setup(repo => repo.GetQueryable()).Returns(mockQueryable);

        // Act
        var result = await _eventService.GetEventsPaginatedAsync(pageNumber, pageSize, null, null, null, dateFrom, dateTo, null);

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
            new() { Id = Guid.NewGuid(), EventName = "Event Yesterday", EventStartDate = now.AddDays(-1), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } },
            new() { Id = Guid.NewGuid(), EventName = "Event In 5 Days", EventStartDate = now.AddDays(5), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } }
        }.AsQueryable();

        var mockQueryable = allEntities.AsQueryable().BuildMock();
        _mockEventRepository.Setup(repo => repo.GetQueryable()).Returns(mockQueryable);

        // Act
        var result = await _eventService.GetEventsPaginatedAsync(pageNumber, pageSize, null, null, dateFilter, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Events.Count);
        Assert.Equal("Event Today", result.Events[0].EventName);
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
            new() { Id = Guid.NewGuid(), EventName = "Event Today", EventStartDate = now, Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } },
            new() { Id = Guid.NewGuid(), EventName = "Event Tomorrow", EventStartDate = now.AddDays(1), Category = new CategoryEntity { Id = Guid.NewGuid(), CategoryName = "Cat" } }
        }.AsQueryable();

        var mockQueryable = allEntities.AsQueryable().BuildMock();
        _mockEventRepository.Setup(repo => repo.GetQueryable()).Returns(mockQueryable);

        // Act
        var result = await _eventService.GetEventsPaginatedAsync(pageNumber, pageSize, null, null, dateFilter, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Events.Count);
        Assert.Equal("Event Yesterday", result.Events[0].EventName);
        Assert.Equal("Event 5 Days Ago", result.Events[1].EventName);
        Assert.True(result.Events.All(e => e.EventStartDate < now));
    }
}