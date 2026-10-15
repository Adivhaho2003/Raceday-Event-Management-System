using Microsoft.AspNetCore.Mvc;
using RaceDay_Management_System.Database;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Controllers
{
    /// <summary>
    /// Participant enrolment endpoints: enrol, view, update, cancel enrolments.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly EnrolmentDatabase _enrolmentDb = new EnrolmentDatabase();
        private readonly EventDatabase _eventDb = new EventDatabase();
        private readonly EventCategoryDatabase _eventCategoryDb = new EventCategoryDatabase();

        /// <summary>
        /// Gets all enrolments. Restricted to Organisers and Admins.
        /// </summary>
        /// <response code="200">Array of all enrolments.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Insufficient permissions.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetAllEnrolments()
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Organiser" && current_role != "Admin")
                return StatusCode(403, new { message = "Forbidden - Organisers only" });

            return Ok(_enrolmentDb.get_all_enrolments());
        }

        /// <summary>
        /// Gets a single enrolment by ID.
        /// </summary>
        /// <param name="id">The enrolment ID.</param>
        /// <response code="200">Enrolment returned.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not your enrolment or event.</response>
        /// <response code="404">Enrolment not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetEnrolment(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });

            Enrolment en = _enrolmentDb.get_enrolment_by_id(id);
            if (en == null) return NotFound(new { message = "Enrolment not found" });

            if (current_role == "Participant" && en.ParticipantId != current_user_id)
                return StatusCode(403, new { message = "Forbidden - not your enrolment" });

            if (current_role == "Organiser")
            {
                Event ev = _eventDb.get_event_by_id(en.EventId);
                if (ev == null || ev.OrganiserId != current_user_id)
                    return StatusCode(403, new { message = "Forbidden - not your event" });
            }

            return Ok(en);
        }

        /// <summary>
        /// Creates a new enrolment for the logged-in Participant.
        /// </summary>
        /// <param name="event_id">The event ID to enrol in.</param>
        /// <param name="event_category_id">The EventCategoryId (from /api/events/{id}/categories).</param>
        /// <response code="201">Enrolment created; returns registration number.</response>
        /// <response code="400">Event not open, deadline passed, etc.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not a Participant.</response>
        /// <response code="404">Event or category not found.</response>
        /// <response code="409">Already enrolled in this event.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult CreateEnrolment(int event_id, int event_category_id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Participant") return StatusCode(403, new { message = "Forbidden - Participants only" });

            Event ev = _eventDb.get_event_by_id(event_id);
            if (ev == null) return NotFound(new { message = "Event not found" });

            if (ev.Status != "Open") return BadRequest(new { message = "Event is not open for registration" });
            if (ev.RegistrationDeadline.HasValue && ev.RegistrationDeadline.Value < DateTime.Now)
                return BadRequest(new { message = "Registration deadline has passed" });

            if (_enrolmentDb.already_enrolled(event_id, current_user_id.Value))
                return Conflict(new { message = "You are already enrolled in this event" });

            var eventCategories = _eventCategoryDb.get_event_categories(event_id);
            var target = eventCategories.FirstOrDefault(ec => ec.EventCategoryId == event_category_id);
            if (target == null) return NotFound(new { message = "Category not available for this event" });

            string reg_number = _enrolmentDb.generate_registration_number();
            if (string.IsNullOrEmpty(reg_number))
                return StatusCode(500, new { message = "Failed to generate registration number" });

            string result = _enrolmentDb.create_enrolment(event_id, current_user_id.Value, event_category_id, reg_number);
            if (result.Contains("successfully"))
                return Created("", new { message = result, registrationNumber = reg_number });

            return BadRequest(new { message = result });
        }

        /// <summary>
        /// Updates an enrolment's status and payment status.
        /// </summary>
        /// <param name="id">The enrolment ID.</param>
        /// <param name="status">New status (Registered, Confirmed, DNS, DNF, Completed).</param>
        /// <param name="payment_status">New payment status (Pending, Paid, Refunded).</param>
        /// <response code="200">Enrolment updated.</response>
        /// <response code="400">Invalid status or payment status.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not your enrolment or event.</response>
        /// <response code="404">Enrolment not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateEnrolment(int id, string status, string payment_status)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });

            Enrolment existing = _enrolmentDb.get_enrolment_by_id(id);
            if (existing == null) return NotFound(new { message = "Enrolment not found" });

            if (current_role == "Participant" && existing.ParticipantId != current_user_id)
                return StatusCode(403, new { message = "Forbidden - not your enrolment" });

            if (current_role == "Organiser")
            {
                Event ev = _eventDb.get_event_by_id(existing.EventId);
                if (ev == null || ev.OrganiserId != current_user_id)
                    return StatusCode(403, new { message = "Forbidden - not your event" });
            }

            string[] valid_status = { "Registered", "Confirmed", "DNS", "DNF", "Completed" };
            if (!valid_status.Contains(status))
                return BadRequest(new { message = "Invalid status. Must be one of: Registered, Confirmed, DNS, DNF, Completed" });

            string[] valid_payment = { "Pending", "Paid", "Refunded" };
            if (!valid_payment.Contains(payment_status))
                return BadRequest(new { message = "Invalid payment status. Must be one of: Pending, Paid, Refunded" });

            string result = _enrolmentDb.update_enrolment(id, status, payment_status);
            if (result.Contains("successfully")) return Ok(new { message = result });
            return BadRequest(new { message = result });
        }

        /// <summary>
        /// Cancels (deletes) an enrolment.
        /// </summary>
        /// <param name="id">The enrolment ID.</param>
        /// <response code="204">Enrolment cancelled.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not your enrolment or event.</response>
        /// <response code="404">Enrolment not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteEnrolment(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });

            Enrolment existing = _enrolmentDb.get_enrolment_by_id(id);
            if (existing == null) return NotFound(new { message = "Enrolment not found" });

            if (current_role == "Participant" && existing.ParticipantId != current_user_id)
                return StatusCode(403, new { message = "Forbidden - not your enrolment" });

            if (current_role == "Organiser")
            {
                Event ev = _eventDb.get_event_by_id(existing.EventId);
                if (ev == null || ev.OrganiserId != current_user_id)
                    return StatusCode(403, new { message = "Forbidden - not your event" });
            }

            string result = _enrolmentDb.delete_enrolment(id);
            if (result.Contains("successfully")) return NoContent();
            return BadRequest(new { message = result });
        }

        /// <summary>
        /// Gets all enrolments for a specific event. Restricted to the event owner (Organiser).
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <response code="200">Array of enrolments for the event.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("event/{eventId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetEnrolmentsByEvent(int eventId)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null) return Unauthorized(new { message = "Not authenticated" });
            if (current_role != "Organiser") return StatusCode(403, new { message = "Forbidden - Organisers only" });

            Event ev = _eventDb.get_event_by_id(eventId);
            if (ev == null) return NotFound(new { message = "Event not found" });
            if (ev.OrganiserId != current_user_id)
                return StatusCode(403, new { message = "Forbidden - not your event" });

            return Ok(_enrolmentDb.get_enrolments_by_event(eventId));
        }
    }
}