using Microsoft.AspNetCore.Mvc;
using RaceDay_Management_System.Database;
using RaceDay_Management_System;
using RaceDay_Management_System.Enums;

namespace RaceDay_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly EventDatabase _eventDb = new EventDatabase();

        // ============================================================
        // GET: api/events
        // Public — anyone can view events
        // ============================================================
        [HttpGet]
        public IActionResult GetAllEvents()
        {
            var events = _eventDb.get_all_events();
            return Ok(events);
        }

        // ============================================================
        // GET: api/events/{id}
        // Public — anyone can view event details
        // ============================================================
        [HttpGet("{id}")]
        public IActionResult GetEvent(int id)
        {
            Event ev = _eventDb.get_event_by_id(id);

            if (ev == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            return Ok(ev);
        }

        // ============================================================
        // POST: api/events
        // Role Required: Organiser
        // ============================================================
        [HttpPost]
        public IActionResult CreateEvent(string event_name,
                                          string description,
                                          string location,
                                          string venue,
                                          string event_date,
                                          string registration_deadline,
                                          decimal entry_fee,
                                          int max_participants,
                                          string event_type,
                                          string distance)
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

            if (event_type != "Running" && event_type != "Walking" && event_type != "Cycling")
            {
                return BadRequest(new { message = "EventType must be 'Running', 'Walking', or 'Cycling'" });
            }

            string result = _eventDb.create_event(event_name, description, current_user_id.Value,
                                                    location, venue, event_date, registration_deadline,
                                                    entry_fee, max_participants, event_type, distance);

            if (result.Contains("successfully"))
            {
                return Created("", new { message = result });
            }

            return BadRequest(new { message = result });
        }

        // ============================================================
        // PUT: api/events/{id}
        // Role Required: Organiser (owner)
        // ============================================================
        [HttpPut("{id}")]
        public IActionResult UpdateEvent(int id,
                                          string event_name,
                                          string description,
                                          string location,
                                          string venue,
                                          string event_date,
                                          string registration_deadline,
                                          decimal entry_fee,
                                          int max_participants,
                                          string status)
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

            Event existing = _eventDb.get_event_by_id(id);

            if (existing == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            if (existing.OrganiserId != current_user_id)
            {
                return StatusCode(403, new { message = "Forbidden - not your event" });
            }

            string result = _eventDb.update_event(id, event_name, description, location, venue,
                                                    event_date, registration_deadline,
                                                    entry_fee, max_participants, status);

            if (result.Contains("successfully"))
            {
                return Ok(new { message = result });
            }

            return BadRequest(new { message = result });
        }

        // ============================================================
        // PATCH: api/events/{id}/status
        // Role Required: Organiser (owner)
        // ============================================================
        [HttpPatch("{id}/status")]
        public IActionResult UpdateEventStatus(int id, string status)
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

            Event existing = _eventDb.get_event_by_id(id);

            if (existing == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            if (existing.OrganiserId != current_user_id)
            {
                return StatusCode(403, new { message = "Forbidden - not your event" });
            }

            // Validate status value
            string[] valid_statuses = { "Pending", "Open", "Closed", "Cancelled", "Completed" };
            if (!valid_statuses.Contains(status))
            {
                return BadRequest(new { message = "Invalid status. Must be one of: Pending, Open, Closed, Cancelled, Completed" });
            }

            string result = _eventDb.update_event_status(id, status);

            if (result.Contains("successfully"))
            {
                return Ok(new { message = result });
            }

            return BadRequest(new { message = result });
        }

        // ============================================================
        // DELETE: api/events/{id}
        // Role Required: Organiser (owner)
        // ============================================================
        [HttpDelete("{id}")]
        public IActionResult DeleteEvent(int id)
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

            Event existing = _eventDb.get_event_by_id(id);

            if (existing == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            if (existing.OrganiserId != current_user_id)
            {
                return StatusCode(403, new { message = "Forbidden - not your event" });
            }

            string result = _eventDb.delete_event(id);

            if (result.Contains("successfully"))
            {
                return NoContent();
            }

            return BadRequest(new { message = result });
        }
    }
}