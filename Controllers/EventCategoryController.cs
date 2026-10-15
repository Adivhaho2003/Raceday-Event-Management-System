using Microsoft.AspNetCore.Mvc;
using RaceDay_Management_System.Database;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Controllers
{
    /// <summary>
    /// Manages links between events and categories (junction table).
    /// </summary>
    [ApiController]
    [Route("api/events/{eventId}/categories")]
    public class EventCategoryController : ControllerBase
    {
        private readonly EventCategoryDatabase _eventCategoryDb = new EventCategoryDatabase();
        private readonly EventDatabase _eventDb = new EventDatabase();
        private readonly CategoryDatabase _categoryDb = new CategoryDatabase();

        /// <summary>
        /// Gets all categories linked to an event. Public endpoint.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <response code="200">Array of categories with fees and capacities.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetEventCategories(int eventId)
        {
            Event ev = _eventDb.get_event_by_id(eventId);
            if (ev == null) return NotFound(new { message = "Event not found" });
            return Ok(_eventCategoryDb.get_event_categories(eventId));
        }

        /// <summary>
        /// Links a category to an event. Restricted to the event owner (Organiser).
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="category_id">The category ID to link.</param>
        /// <param name="fee">Optional custom fee for this event-category link.</param>
        /// <param name="capacity">Optional capacity for this event-category link.</param>
        /// <response code="201">Category linked to event.</response>
        /// <response code="400">Validation errors.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event or category not found.</response>
        /// <response code="409">Category already linked.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult AddCategoryToEvent(int eventId, int category_id, decimal? fee, int? capacity)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Organiser") return StatusCode(403, new { message = "Forbidden - Organisers only" });

            Event ev = _eventDb.get_event_by_id(eventId);
            if (ev == null) return NotFound(new { message = "Event not found" });
            if (ev.OrganiserId != current_user_id) return StatusCode(403, new { message = "Forbidden - not your event" });

            Category cat = _categoryDb.get_category_by_id(category_id);
            if (cat == null) return NotFound(new { message = "Category not found" });

            if (_eventCategoryDb.link_exists(eventId, category_id))
                return Conflict(new { message = "Category already linked to this event" });

            if (capacity.HasValue && capacity.Value <= 0)
                return BadRequest(new { message = "Capacity must be greater than 0" });

            string result = _eventCategoryDb.add_category_to_event(eventId, category_id, fee, capacity);
            if (result.Contains("successfully")) return Created("", new { message = result });
            return BadRequest(new { message = result });
        }

        /// <summary>
        /// Updates the fee or capacity for a category linked to an event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="categoryId">The category ID.</param>
        /// <param name="fee">New fee (nullable).</param>
        /// <param name="capacity">New capacity (nullable, must be > 0).</param>
        /// <response code="200">Link updated.</response>
        /// <response code="400">Validation errors.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event, category, or link not found.</response>
        [HttpPut("{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateEventCategory(int eventId, int categoryId, decimal? fee, int? capacity)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Organiser") return StatusCode(403, new { message = "Forbidden - Organisers only" });

            Event ev = _eventDb.get_event_by_id(eventId);
            if (ev == null) return NotFound(new { message = "Event not found" });
            if (ev.OrganiserId != current_user_id) return StatusCode(403, new { message = "Forbidden - not your event" });

            var eventCategories = _eventCategoryDb.get_event_categories(eventId);
            EventCategory target = eventCategories.FirstOrDefault(ec => ec.CategoryId == categoryId);
            if (target == null) return NotFound(new { message = "Category not linked to this event" });

            if (capacity.HasValue && capacity.Value <= 0)
                return BadRequest(new { message = "Capacity must be greater than 0" });

            string result = _eventCategoryDb.update_event_category(target.EventCategoryId, fee, capacity);
            if (result.Contains("successfully")) return Ok(new { message = result });
            return BadRequest(new { message = result });
        }

        /// <summary>
        /// Removes a category from an event. Restricted to the event owner.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="categoryId">The category ID.</param>
        /// <response code="204">Category unlinked.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event or link not found.</response>
        [HttpDelete("{categoryId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult RemoveCategoryFromEvent(int eventId, int categoryId)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Organiser") return StatusCode(403, new { message = "Forbidden - Organisers only" });

            Event ev = _eventDb.get_event_by_id(eventId);
            if (ev == null) return NotFound(new { message = "Event not found" });
            if (ev.OrganiserId != current_user_id) return StatusCode(403, new { message = "Forbidden - not your event" });

            if (!_eventCategoryDb.link_exists(eventId, categoryId))
                return NotFound(new { message = "Category not linked to this event" });

            string result = _eventCategoryDb.remove_category_from_event(eventId, categoryId);
            if (result.Contains("successfully")) return NoContent();
            return BadRequest(new { message = result });
        }
    }
}