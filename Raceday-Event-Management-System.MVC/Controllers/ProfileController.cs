using Microsoft.AspNetCore.Mvc;
using Raceday_Event_Management_System.MVC.Models.ViewModels;
using Raceday_Event_Management_System.MVC.Services;

namespace Raceday_Event_Management_System.MVC.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApiClient _api;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EventService _eventService;

        public ProfileController(ApiClient api, IWebHostEnvironment webHostEnvironment, EventService eventService)
        {
            _api = api;
            _webHostEnvironment = webHostEnvironment;
            _eventService = eventService;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync<ProfileViewModel>($"/api/users/{userId}");
            if (response == null || !response.IsSuccess) return NotFound("User not found");

            return View(response.Data);
        }

        // POST: /Profile/Update
        [HttpPost]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");
            if (!ModelState.IsValid) return View("Index", model);

            var endpoint = $"/api/users/{userId}?first_name={Uri.EscapeDataString(model.FirstName)}" +
                           $"&last_name={Uri.EscapeDataString(model.LastName)}" +
                           $"&phone={Uri.EscapeDataString(model.Phone ?? "")}" +
                           $"&date_of_birth={model.DateOfBirth?.ToString("yyyy-MM-dd") ?? ""}";

            var response = await _api.PutAsync<object>(endpoint);

            if (response != null && response.IsSuccess)
            {
                HttpContext.Session.SetString("FirstName", model.FirstName);
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update profile.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Profile/UploadPicture
        [HttpPost]
        public async Task<IActionResult> UploadPicture(IFormFile profilePicture)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // Ensure only participants can upload
            if (HttpContext.Session.GetString("Role") != "Participant")
            {
                TempData["ErrorMessage"] = "Only participants can upload profile pictures.";
                return RedirectToAction(nameof(Index));
            }

            if (profilePicture != null && profilePicture.Length > 0)
            {
                // 1. Save the file locally to wwwroot/images/profiles
                var imageUrl = await _eventService.SaveImageAsync(profilePicture, _webHostEnvironment.WebRootPath, "profiles");

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    // 2. Send the URL string to the API
                    var endpoint = $"/api/users/{userId}/profile-picture?profile_picture_url={Uri.EscapeDataString(imageUrl)}";
                    var response = await _api.PostAsync<object>(endpoint);

                    if (response != null && response.IsSuccess)
                    {
                        // 3. Update the session so the navbar shows the new image immediately
                        HttpContext.Session.SetString("ProfilePictureUrl", imageUrl);
                        TempData["SuccessMessage"] = "Profile picture updated successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "API failed to save the picture URL. Please make sure the API is running.";
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}