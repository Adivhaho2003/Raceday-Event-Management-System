using Microsoft.AspNetCore.Mvc;
using Raceday_Event_Management_System.MVC.Models.ViewModels;
using Raceday_Event_Management_System.MVC.Services;

namespace Raceday_Event_Management_System.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly EventService _eventService;
        private readonly ApiClient _api;

        public ParticipantController(EventService eventService, ApiClient api)
        {
            _eventService = eventService;
            _api = api;
        }

        public async Task<IActionResult> EventDetails(int id)
        {
            var model = await _eventService.GetEventDetailsAsync(id);
            if (model == null) return NotFound("Event not found.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int eventCategoryId)
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
            {
                TempData["ErrorMessage"] = "You must be logged in as a Participant to enrol.";
                return RedirectToAction("Login", "Account");
            }

            var response = await _eventService.EnrolInEventAsync(eventId, eventCategoryId);

            if (response != null && response.IsSuccess)
                TempData["SuccessMessage"] = "Successfully enrolled in the event!";
            else
                TempData["ErrorMessage"] = "Failed to enrol. You may already be enrolled or the event is full.";

            return RedirectToAction(nameof(EventDetails), new { id = eventId });
        }

        // GET: /Participant/MyEvents
        public async Task<IActionResult> MyEvents()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Account");

            // Fetch the user's enrolments from the API
            var response = await _api.GetAsync<List<dynamic>>($"/api/users/{userId}/enrolments");

            ViewBag.Enrolments = response?.Data ?? new List<dynamic>();
            return View();
        }

        // GET: /Participant/MyResults
        public async Task<IActionResult> MyResults()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync<List<dynamic>>($"/api/users/{userId}/results");

            ViewBag.Results = response?.Data ?? new List<dynamic>();
            return View();
        }

        // GET: /Participant/Profile
        public async Task<IActionResult> Profile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync<dynamic>($"/api/users/{userId}");
            if (response == null || !response.IsSuccess) return NotFound();

            ViewBag.Profile = response.Data;
            return View();
        }
    }
}