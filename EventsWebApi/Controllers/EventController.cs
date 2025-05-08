using EventsWebApi.ApiModels.Requests;
using EventsWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventsWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController(IEventService eventService, ILogger<EventController> logger) : ControllerBase
    {
        private readonly IEventService _eventService = eventService;
        private readonly ILogger<EventController> _logger = logger; // Tillagd logger

        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventRequest requestData)
        {
            _logger.LogInformation("Attempting to create event with name: {EventName}", requestData.EventName);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("CreateEvent failed due to invalid model state for event: {EventName}", requestData.EventName);
                    return BadRequest(ModelState);
                }

                var result = await _eventService.CreateEventAsync(requestData);

                if (result == null)
                {
                    _logger.LogWarning("Service failed to create event with name: {EventName}", requestData.EventName);
                    return BadRequest("Failed to create an event");
                }

                _logger.LogInformation("Event created successfully with ID: {EventId} for event name: {EventName}", result.Id, requestData.EventName);
                return CreatedAtAction(nameof(GetEvent), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating event with name: {EventName}", requestData.EventName);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing your request.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            _logger.LogInformation("Attempting to retrieve all events.");
            try
            {
                var result = await _eventService.GetAllEventsAsync();
                _logger.LogInformation("Successfully retrieved all events.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving all events.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing your request.");
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetEvent(Guid id)
        {
            _logger.LogInformation("Attempting to retrieve event with ID: {EventId}", id);
            try
            {
                var result = await _eventService.GetEventByIdAsync(id);

                if (result == null)
                {
                    _logger.LogWarning("Event with ID: {EventId} not found.", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved event with ID: {EventId}", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving event with ID: {EventId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing your request.");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            _logger.LogInformation("Attempting to delete event with ID: {EventId}", id);
            try
            {
                var result = await _eventService.DeleteEventAsync(id);

                if (!result)
                {
                    _logger.LogWarning("Service failed to delete event or event with ID: {EventId} not found.", id);
                    return NotFound();
                }

                _logger.LogInformation("Event with ID: {EventId} deleted successfully.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting event with ID: {EventId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing your request.");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateEvent(Guid id, UpdateEventRequest requestData)
        {
            _logger.LogInformation("Attempting to update event with ID: {EventId}", id);
            try
            {
                if (requestData.Id != Guid.Empty && id != requestData.Id)
                {
                    _logger.LogWarning("UpdateEvent failed for ID: {RouteId}. ID in URL does not match ID in request body: {BodyId}", id, requestData.Id);
                    return BadRequest("ID in URL does not match ID in request body.");
                }

                var result = await _eventService.UpdateEventAsync(id, requestData);

                if (result == null)
                {
                    _logger.LogWarning("Service failed to update event or event with ID: {EventId} not found.", id);
                    return NotFound();
                }

                _logger.LogInformation("Event with ID: {EventId} updated successfully.", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating event with ID: {EventId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing your request.");
            }
        }
    }
}
