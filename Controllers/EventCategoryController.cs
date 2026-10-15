using Microsoft.AspNetCore.Mvc;
using RaceDay_Management_System.Database;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Controllers
{
    [ApiController]
    [Route("api/events/{eventId}/categories")]
    public class EventCategoryController : ControllerBase
    {
        private readonly EventCategoryDatabase _eventCategoryDb = new EventCategoryDatabase();
        private readonly EventDatabase _eventDb = new EventDatabase();
        private readonly CategoryDatabase _categoryDb = new CategoryDatabase();

        // ============================================================
        // GET: api/events/{eventId}/categories
        // Public
        // ============================================================
        [HttpGet]
        public IActionResult GetEventCategories(int eventId)
        {
            Event ev = _eventDb.get_event_by_id(eventId);

            if (ev == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            var categories = _eventCategoryDb.get_event_categories(eventId);
            return Ok(categories);
        }

        // ============================================================
        // POST: api/events/{eventId}/categories
        // Role Required: Organiser (owner)
        // ============================================================
        [HttpPost]
        public IActionResult AddCategoryToEvent(int eventId,
                                                 int category_id,
                                                 decimal? fee,
                                                 int? capacity)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            if (current_role != "Organiser")
            {
                return StatusCode(403, new { message = "Forbidden - Organisers only" });
            }

            // Verify event exists and user owns it
            Event ev = _eventDb.get_event_by_id(eventId);

            if (ev == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            if (ev.OrganiserId != current_user_id)
            {
                return StatusCode(403, new { message = "Forbidden - not your event" });
            }

            // Verify category exists
            Category cat = _categoryDb.get_category_by_id(category_id);

            if (cat == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            // Check for duplicate
            if (_eventCategoryDb.link_exists(eventId, category_id))
            {
                return Conflict(new { message = "Category already linked to this event" });
            }

            // Validate capacity
            if (capacity.HasValue && capacity.Value <= 0)
            {
                return BadRequest(new { message = "Capacity must be greater than 0" });
            }

            string result = _eventCategoryDb.add_category_to_event(eventId, category_id, fee, capacity);

            if (result.Contains("successfully"))
            {
                return Created("", new { message = result });
            }

            return BadRequest(new { message = result });
        }

        // ============================================================
        // PUT: api/events/{eventId}/categories/{categoryId}
        // Role Required: Organiser (owner)
        // ============================================================
        [HttpPut("{categoryId}")]
        public IActionResult UpdateEventCategory(int eventId,
                                                  int categoryId,
                                                  decimal? fee,
                                                  int? capacity)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            if (current_role != "Organiser")
            {
                return StatusCode(403, new { message = "Forbidden - Organisers only" });
            }

            // Verify event exists and user owns it
            Event ev = _eventDb.get_event_by_id(eventId);

            if (ev == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            if (ev.OrganiserId != current_user_id)
            {
                return StatusCode(403, new { message = "Forbidden - not your event" });
            }

            // Find the event category link
            var eventCategories = _eventCategoryDb.get_event_categories(eventId);
            EventCategory target = eventCategories.FirstOrDefault(ec => ec.CategoryId == categoryId);

            if (target == null)
            {
                return NotFound(new { message = "Category not linked to this event" });
            }

            // Validate capacity
            if (capacity.HasValue && capacity.Value <= 0)
            {
                return BadRequest(new { message = "Capacity must be greater than 0" });
            }

            string result = _eventCategoryDb.update_event_category(target.EventCategoryId, fee, capacity);

            if (result.Contains("successfully"))
            {
                return Ok(new { message = result });
            }

            return BadRequest(new { message = result });
        }

        // ============================================================
        // DELETE: api/events/{eventId}/categories/{categoryId}
        // Role Required: Organiser (owner)
        // ============================================================
        [HttpDelete("{categoryId}")]
        public IActionResult RemoveCategoryFromEvent(int eventId, int categoryId)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            if (current_role != "Organiser")
            {
                return StatusCode(403, new { message = "Forbidden - Organisers only" });
            }

            // Verify event exists and user owns it
            Event ev = _eventDb.get_event_by_id(eventId);

            if (ev == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            if (ev.OrganiserId != current_user_id)
            {
                return StatusCode(403, new { message = "Forbidden - not your event" });
            }

            // Verify the link exists
            if (!_eventCategoryDb.link_exists(eventId, categoryId))
            {
                return NotFound(new { message = "Category not linked to this event" });
            }

            string result = _eventCategoryDb.remove_category_from_event(eventId, categoryId);

            if (result.Contains("successfully"))
            {
                return NoContent();
            }

            return BadRequest(new { message = result });
        }
    }
}