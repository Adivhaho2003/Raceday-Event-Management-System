using Raceday_Event_Management_System.MVC.Models;
using Raceday_Event_Management_System.MVC.Models.ViewModels;

namespace Raceday_Event_Management_System.MVC.Services
{
    public class EventService
    {
        private readonly ApiClient _api;

        public EventService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<EventViewModel>> GetAllEventsAsync()
        {
            var response = await _api.GetAsync<List<EventViewModel>>("/api/events");
            return response?.IsSuccess == true && response.Data != null ? response.Data : new List<EventViewModel>();
        }

        public async Task<EventDetailsViewModel?> GetEventDetailsAsync(int eventId)
        {
            // 1. Get the event details
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

            // 2. Get the categories for this event
            var categoriesResponse = await _api.GetAsync<List<EventCategoryItem>>($"/api/events/{eventId}/categories");
            if (categoriesResponse?.IsSuccess == true && categoriesResponse.Data != null)
            {
                viewModel.Categories = categoriesResponse.Data;
            }

            return viewModel;
        }

        public async Task<ApiResponse<object>?> EnrolInEventAsync(int eventId, int eventCategoryId)
        {
            // Note: The API expects event_id and event_category_id as query parameters
            var endpoint = $"/api/enrolments?event_id={eventId}&event_category_id={eventCategoryId}";
            return await _api.PostAsync<object>(endpoint);
        }
    }
}