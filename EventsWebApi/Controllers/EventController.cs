using EventsWebApi.ApiModels.DTO;
using EventsWebApi.ApiModels.Requests;
using EventsWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventsWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController(IEventService eventService, ILogger<EventController> logger) : ControllerBase
    {
        private readonly IEventService _eventService = eventService;
        private readonly ILogger<EventController> _logger = logger;

        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventRequest requestData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _eventService.CreateEventAsync(requestData);

                if (result == null)
                {
                    return BadRequest("Failed to create an event");
                }

                return CreatedAtAction(nameof(GetEventById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing your request.");
            }
        }

        [HttpGet("all")]
        public async Task <IActionResult> GetAllEvents()
        {
            try
            {
                var result = await _eventService.GetAllEventsAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing your request.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPagingEvents([FromQuery] GetEventQuery queryParams)
        {
            try
            {
                var result = await _eventService.GetEventsPaginatedAsync(
                    queryParams.PageNumber, 
                    queryParams.PageSize, 
                    queryParams.CategoryNameFilter,
                    queryParams.SearchTerm, 
                    queryParams.DateFilter, 
                    queryParams.SpecificDateFrom, 
                    queryParams.SpecificDateTo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing your request.");
            }
        }

        [HttpGet("{id:guid}", Name = "GetEventById")]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            try
            {
                var result = await _eventService.GetEventByIdAsync(id);

                if (result == null)
                {
                    _logger.LogWarning("Event with ID: {EventId} not found.", id);
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing your request.");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            try
            {
                var result = await _eventService.DeleteEventAsync(id);

                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing your request.");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateEvent(Guid id, UpdateEventRequest requestData)
        {
            try
            {
                if (requestData.Id != Guid.Empty && id != requestData.Id)
                {
                    return BadRequest("ID in URL does not match ID in request body.");
                }

                var result = await _eventService.UpdateEventAsync(id, requestData);

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing your request.");
            }
        }
    }
}
