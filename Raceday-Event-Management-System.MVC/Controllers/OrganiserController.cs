using Microsoft.AspNetCore.Mvc;
using Raceday_Event_Management_System.MVC.Models.ViewModels;
using Raceday_Event_Management_System.MVC.Services;

namespace Raceday_Event_Management_System.MVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly EventService _eventService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public OrganiserController(EventService eventService, IWebHostEnvironment webHostEnvironment)
        {
            _eventService = eventService;
            _webHostEnvironment = webHostEnvironment;
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("Role") == "Organiser";
        }

        // GET: /Organiser/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var events = await _eventService.GetOrganiserEventsAsync(userId.Value);

            var model = new OrganiserDashboardViewModel
            {
                TotalEvents = events.Count,
                RecentEvents = events.OrderByDescending(e => e.EventDate).Take(5).ToList()
            };

            return View(model);
        }

        // GET: /Organiser/ManageEvents
        public async Task<IActionResult> ManageEvents()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var events = await _eventService.GetOrganiserEventsAsync(userId.Value);

            return View(events);
        }

        // GET: /Organiser/CreateEvent
        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");
            return View(new CreateEventViewModel());
        }

        // POST: /Organiser/CreateEvent
        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");
            if (!ModelState.IsValid) return View(model);

            string? imageUrl = null;
            if (model.BannerImage != null)
            {
                imageUrl = await _eventService.SaveImageAsync(model.BannerImage, _webHostEnvironment.WebRootPath);
            }

            var response = await _eventService.CreateEventAsync(model, imageUrl);

            if (response != null && response.IsSuccess)
            {
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(ManageEvents));
            }

            string errorMsg = response?.ErrorMessage ?? "Unknown error";
            if (errorMsg.StartsWith("{"))
                errorMsg = "The API rejected the request. Please check all fields.";

            ModelState.AddModelError(string.Empty, $"Failed to create event: {errorMsg}");
            return View(model);
        }

        // GET: /Organiser/ManageCategories/5
        public async Task<IActionResult> ManageCategories(int id)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var events = await _eventService.GetOrganiserEventsAsync(userId.Value);
            var targetEvent = events.FirstOrDefault(e => e.EventId == id);

            if (targetEvent == null) return NotFound("Event not found or you don't have permission.");

            var model = new ManageCategoriesViewModel
            {
                EventId = targetEvent.EventId,
                EventName = targetEvent.EventName,
                LinkedCategories = await _eventService.GetEventCategoriesAsync(id),
                AvailableCategories = await _eventService.GetAllCategoriesAsync()
            };

            return View(model);
        }

        // POST: /Organiser/AddCategory
        [HttpPost]
        public async Task<IActionResult> AddCategory(ManageCategoriesViewModel model)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                model.LinkedCategories = await _eventService.GetEventCategoriesAsync(model.EventId);
                model.AvailableCategories = await _eventService.GetAllCategoriesAsync();
                return View("ManageCategories", model);
            }

            var response = await _eventService.LinkCategoryToEventAsync(model.EventId, model.SelectedCategoryId, model.Fee, model.Capacity);

            if (response != null && response.IsSuccess)
                TempData["SuccessMessage"] = "Category linked successfully!";
            else
                TempData["ErrorMessage"] = "Failed to link category. It might already be linked.";

            return RedirectToAction(nameof(ManageCategories), new { id = model.EventId });
        }

        // GET: /Organiser/Enrolments/5
        public async Task<IActionResult> Enrolments(int id)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var events = await _eventService.GetOrganiserEventsAsync(userId.Value);
            var targetEvent = events.FirstOrDefault(e => e.EventId == id);
            if (targetEvent == null) return NotFound();

            ViewBag.EventName = targetEvent.EventName;
            ViewBag.EventId = targetEvent.EventId;

            var enrolments = await _eventService.GetEventEnrolmentsAsync(id);
            return View(enrolments);
        }

        // POST: /Organiser/CaptureResult
        [HttpPost]
        public async Task<IActionResult> CaptureResult(CaptureResultViewModel model, int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(model.GunTime) || string.IsNullOrEmpty(model.ChipTime))
            {
                TempData["ErrorMessage"] = "Time is required.";
                return RedirectToAction(nameof(Enrolments), new { id = eventId });
            }

            var response = await _eventService.CreateResultAsync(
                model.EnrolmentId,
                model.GunTime,
                model.ChipTime,
                model.OverallPosition ?? 0,
                model.CategoryPosition ?? 0,
                model.Pace ?? 0,
                model.Status);

            if (response != null && response.IsSuccess)
                TempData["SuccessMessage"] = "Result captured successfully!";
            else
                TempData["ErrorMessage"] = $"Failed to capture result: {response?.ErrorMessage}";

            return RedirectToAction(nameof(Enrolments), new { id = eventId });
        }
    }
}