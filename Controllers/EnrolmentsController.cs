using Microsoft.AspNetCore.Mvc;
using RaceDay_Management_System.Database;
using RaceDay_Management_System;

namespace RaceDay_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly EnrolmentDatabase _enrolmentDb = new EnrolmentDatabase();
        private readonly EventDatabase _eventDb = new EventDatabase();
        private readonly EventCategoryDatabase _eventCategoryDb = new EventCategoryDatabase();

        // ============================================================
        // GET: api/enrolments
        // Role Required: Organiser / Admin
        // ============================================================
        [HttpGet]
        public IActionResult GetAllEnrolments()
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            if (current_role != "Organiser" && current_role != "Admin")
            {
                return StatusCode(403, new { message = "Forbidden - Organisers only" });
            }

            var enrolments = _enrolmentDb.get_all_enrolments();
            return Ok(enrolments);
        }

        // ============================================================
        // GET: api/enrolments/{id}
        // Role Required: Participant (own) / Organiser (own events)
        // ============================================================
        [HttpGet("{id}")]
        public IActionResult GetEnrolment(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            Enrolment en = _enrolmentDb.get_enrolment_by_id(id);

            if (en == null)
            {
                return NotFound(new { message = "Enrolment not found" });
            }

            // Participant can only view their own enrolments
            if (current_role == "Participant" && en.ParticipantId != current_user_id)
            {
                return StatusCode(403, new { message = "Forbidden - not your enrolment" });
            }

            // Organiser can only view enrolments for their own events
            if (current_role == "Organiser")
            {
                Event ev = _eventDb.get_event_by_id(en.EventId);
                if (ev == null || ev.OrganiserId != current_user_id)
                {
                    return StatusCode(403, new { message = "Forbidden - not your event" });
                }
            }

            return Ok(en);
        }

        // ============================================================
        // POST: api/enrolments
        // Role Required: Participant
        // ============================================================
        [HttpPost]
        public IActionResult CreateEnrolment(int event_id, int event_category_id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            if (current_role != "Participant")
            {
                return StatusCode(403, new { message = "Forbidden - Participants only" });
            }

            // Check event exists
            Event ev = _eventDb.get_event_by_id(event_id);

            if (ev == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            // Check event is open
            if (ev.Status != "Open")
            {
                return BadRequest(new { message = "Event is not open for registration" });
            }

            // Check registration deadline
            if (ev.RegistrationDeadline.HasValue && ev.RegistrationDeadline.Value < DateTime.Now)
            {
                return BadRequest(new { message = "Registration deadline has passed" });
            }

            // Check for duplicate enrolment
            if (_enrolmentDb.already_enrolled(event_id, current_user_id.Value))
            {
                return Conflict(new { message = "You are already enrolled in this event" });
            }

            // Verify event category link exists
            var eventCategories = _eventCategoryDb.get_event_categories(event_id);
            var targetCategory = eventCategories.FirstOrDefault(ec => ec.EventCategoryId == event_category_id);

            if (targetCategory == null)
            {
                return NotFound(new { message = "Category not available for this event" });
            }

            // Generate unique registration number
            string reg_number = _enrolmentDb.generate_registration_number();

            if (string.IsNullOrEmpty(reg_number))
            {
                return StatusCode(500, new { message = "Failed to generate registration number" });
            }

            string result = _enrolmentDb.create_enrolment(event_id, current_user_id.Value, event_category_id, reg_number);

            if (result.Contains("successfully"))
            {
                return Created("", new
                {
                    message = result,
                    registrationNumber = reg_number
                });
            }

            return BadRequest(new { message = result });
        }

        // ============================================================
        // PUT: api/enrolments/{id}
        // Role Required: Participant (own) / Organiser (own events)
        // ============================================================
        [HttpPut("{id}")]
        public IActionResult UpdateEnrolment(int id, string status, string payment_status)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            Enrolment existing = _enrolmentDb.get_enrolment_by_id(id);

            if (existing == null)
            {
                return NotFound(new { message = "Enrolment not found" });
            }

            // Participant can only update their own enrolment
            if (current_role == "Participant" && existing.ParticipantId != current_user_id)
            {
                return StatusCode(403, new { message = "Forbidden - not your enrolment" });
            }

            // Organiser can only update enrolments for their own events
            if (current_role == "Organiser")
            {
                Event ev = _eventDb.get_event_by_id(existing.EventId);
                if (ev == null || ev.OrganiserId != current_user_id)
                {
                    return StatusCode(403, new { message = "Forbidden - not your event" });
                }
            }

            // Validate status
            string[] valid_statuses = { "Registered", "Confirmed", "DNS", "DNF", "Completed" };
            if (!valid_statuses.Contains(status))
            {
                return BadRequest(new { message = "Invalid status. Must be one of: Registered, Confirmed, DNS, DNF, Completed" });
            }

            // Validate payment status
            string[] valid_payment = { "Pending", "Paid", "Refunded" };
            if (!valid_payment.Contains(payment_status))
            {
                return BadRequest(new { message = "Invalid payment status. Must be one of: Pending, Paid, Refunded" });
            }

            string result = _enrolmentDb.update_enrolment(id, status, payment_status);

            if (result.Contains("successfully"))
            {
                return Ok(new { message = result });
            }

            return BadRequest(new { message = result });
        }

        // ============================================================
        // DELETE: api/enrolments/{id}
        // Role Required: Participant (own) / Organiser (own events)
        // ============================================================
        [HttpDelete("{id}")]
        public IActionResult DeleteEnrolment(int id)
        {
            int? current_user_id = HttpContext.Session.GetInt32("UserId");
            string? current_role = HttpContext.Session.GetString("Role");

            if (current_user_id == null)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            Enrolment existing = _enrolmentDb.get_enrolment_by_id(id);

            if (existing == null)
            {
                return NotFound(new { message = "Enrolment not found" });
            }

            // Participant can only cancel their own enrolment
            if (current_role == "Participant" && existing.ParticipantId != current_user_id)
            {
                return StatusCode(403, new { message = "Forbidden - not your enrolment" });
            }

            // Organiser can only cancel enrolments for their own events
            if (current_role == "Organiser")
            {
                Event ev = _eventDb.get_event_by_id(existing.EventId);
                if (ev == null || ev.OrganiserId != current_user_id)
                {
                    return StatusCode(403, new { message = "Forbidden - not your event" });
                }
            }

            string result = _enrolmentDb.delete_enrolment(id);

            if (result.Contains("successfully"))
            {
                return NoContent();
            }

            return BadRequest(new { message = result });
        }

        // ============================================================
        // GET: api/enrolments/event/{eventId}
        // Role Required: Organiser (owner)
        // ============================================================
        [HttpGet("event/{eventId}")]
        public IActionResult GetEnrolmentsByEvent(int eventId)
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

            Event ev = _eventDb.get_event_by_id(eventId);

            if (ev == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            if (ev.OrganiserId != current_user_id)
            {
                return StatusCode(403, new { message = "Forbidden - not your event" });
            }

            var enrolments = _enrolmentDb.get_enrolments_by_event(eventId);
            return Ok(enrolments);
        }
    }
}