using EventsWebApi.ApiModels.Requests;
using EventsWebApi.ApiModels.Responses;
using EventsWebApi.Data.Entities;
using Riok.Mapperly.Abstractions;

namespace EventsWebApi.Mapper;

[Mapper]
public static partial class EntityMapper
{
    public static partial EventEntity MapToEventEntity(CreateEventRequest source);
    public static partial CategoryEntity MapToCategoryEntity(CreateCategoryRequest source);

}

[Mapper(AllowNullPropertyAssignment = false)]
public static partial class UpdateMapper
{
    public static partial void UpdateCategoryEntity(UpdateCategoryRequest source, CategoryEntity target);
    public static partial void UpdateEventEntity(UpdateEventRequest source, EventEntity target);
}

[Mapper]
public static partial class ResponseMapper
{
    public static partial EventResponse MapToEventResponse(EventEntity source);
    public static partial EventCreatedResponse MapToEventCreatedResponse(EventEntity source);
    public static partial IEnumerable<EventResponse> MapToEventResponseList(IEnumerable<EventEntity> source);
    public static partial CategoryResponse MapToCategoryResponse(CategoryEntity source);
    public static partial IEnumerable<CategoryResponse> MapToCategoryResponseList(IEnumerable<CategoryEntity> source);
}
