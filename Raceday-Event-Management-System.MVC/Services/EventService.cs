using Raceday_Event_Management_System.MVC.Models;
using Raceday_Event_Management_System.MVC.Models.ViewModels;
using System.Text.Json;

namespace Raceday_Event_Management_System.MVC.Services
{
    public class EventService
    {
        private readonly ApiClient _api;

        public EventService(ApiClient api)
        {
            _api = api;
        }

        // ============================================================
        // PUBLIC / PARTICIPANT METHODS
        // ============================================================

        public async Task<List<EventViewModel>> GetAllEventsAsync()
        {
            var response = await _api.GetAsync<List<EventViewModel>>("/api/events");
            return response?.IsSuccess == true && response.Data != null ? response.Data : new List<EventViewModel>();
        }

        public async Task<EventDetailsViewModel?> GetEventDetailsAsync(int eventId)
        {
            var eventResponse = await _api.GetAsync<EventViewModel>($"/api/events/{eventId}");
            if (eventResponse?.IsSuccess != true || eventResponse.Data == null) return null;

            var viewModel = new EventDetailsViewModel
            {
                EventId = eventResponse.Data.EventId,
                EventName = eventResponse.Data.EventName,
                Description = eventResponse.Data.Description,
                Location = eventResponse.Data.Location,
                EventDate = eventResponse.Data.EventDate,
                EventType = eventResponse.Data.EventType,
                Distance = eventResponse.Data.Distance,
                Status = eventResponse.Data.Status
            };

            var categoriesResponse = await _api.GetAsync<List<EventCategoryItem>>($"/api/events/{eventId}/categories");
            if (categoriesResponse?.IsSuccess == true && categoriesResponse.Data != null)
            {
                viewModel.Categories = categoriesResponse.Data;
            }

            return viewModel;
        }

        public async Task<ApiResponse<object>?> EnrolInEventAsync(int eventId, int eventCategoryId)
        {
            var endpoint = $"/api/enrolments?event_id={eventId}&event_category_id={eventCategoryId}";
            return await _api.PostAsync<object>(endpoint);
        }

        // ============================================================
        // ORGANISER METHODS
        // ============================================================

        public async Task<List<EventViewModel>> GetOrganiserEventsAsync(int organiserId)
        {
            var response = await _api.GetAsync<List<EventViewModel>>($"/api/users/{organiserId}/events");
            return response?.IsSuccess == true && response.Data != null ? response.Data : new List<EventViewModel>();
        }

        // Save Image to wwwroot
        public async Task<string?> SaveImageAsync(IFormFile file, string webRootPath)
        {
            if (file == null || file.Length == 0) return null;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension)) return null;

            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(webRootPath, "images", "events");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/events/{fileName}";
        }

        // Create Event (Includes Image URL)
        public async Task<ApiResponse<object>?> CreateEventAsync(CreateEventViewModel model, string? imageUrl)
        {
            var formattedEventDate = model.EventDate.ToString("s");
            var formattedDeadline = model.RegistrationDeadline?.ToString("s") ?? "";

            var endpoint = $"/api/events?" +
                           $"event_name={Uri.EscapeDataString(model.EventName)}" +
                           $"&description={Uri.EscapeDataString(model.Description ?? "")}" +
                           $"&location={Uri.EscapeDataString(model.Location)}" +
                           $"&venue={Uri.EscapeDataString(model.Venue ?? "")}" +
                           $"&event_date={formattedEventDate}" +
                           $"&registration_deadline={formattedDeadline}" +
                           $"&entry_fee={model.EntryFee}" +
                           $"&max_participants={model.MaxParticipants}" +
                           $"&event_type={model.EventType}" +
                           $"&distance={Uri.EscapeDataString(model.Distance ?? "")}";

            if (!string.IsNullOrEmpty(imageUrl))
            {
                endpoint += $"&banner_image_url={Uri.EscapeDataString(imageUrl)}";
            }

            return await _api.PostAsync<object>(endpoint);
        }

        // --- CATEGORY METHODS ---

        public async Task<List<CategoryViewModel>> GetAllCategoriesAsync()
        {
            var response = await _api.GetAsync<List<CategoryViewModel>>("/api/categories");
            return response?.IsSuccess == true && response.Data != null ? response.Data : new List<CategoryViewModel>();
        }

        public async Task<List<EventCategoryItem>> GetEventCategoriesAsync(int eventId)
        {
            var response = await _api.GetAsync<List<EventCategoryItem>>($"/api/events/{eventId}/categories");
            return response?.IsSuccess == true && response.Data != null ? response.Data : new List<EventCategoryItem>();
        }

        public async Task<ApiResponse<object>?> LinkCategoryToEventAsync(int eventId, int categoryId, decimal fee, int capacity)
        {
            var endpoint = $"/api/events/{eventId}/categories?category_id={categoryId}&fee={fee}&capacity={capacity}";
            return await _api.PostAsync<object>(endpoint);
        }

        // --- ENROLMENT & RESULT METHODS ---

        public async Task<List<EventEnrolmentViewModel>> GetEventEnrolmentsAsync(int eventId)
        {
            var response = await _api.GetAsync<List<EventEnrolmentViewModel>>($"/api/enrolments/event/{eventId}");
            return response?.IsSuccess == true && response.Data != null ? response.Data : new List<EventEnrolmentViewModel>();
        }

        public async Task<ApiResponse<object>?> CreateResultAsync(int enrolmentId, string gunTime, string chipTime, int overallPos, int catPos, double pace, string status)
        {
            var formattedGunTime = TimeSpan.Parse(gunTime).ToString(@"hh\:mm\:ss");
            var formattedChipTime = TimeSpan.Parse(chipTime).ToString(@"hh\:mm\:ss");

            var endpoint = $"/api/results?enrolment_id={enrolmentId}" +
                           $"&gun_time={formattedGunTime}" +
                           $"&chip_time={formattedChipTime}" +
                           $"&overall_position={overallPos}" +
                           $"&category_position={catPos}" +
                           $"&pace={pace}" +
                           $"&status={status}";

            return await _api.PostAsync<object>(endpoint);
        }
    }
}