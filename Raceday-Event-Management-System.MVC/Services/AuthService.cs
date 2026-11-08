using Raceday_Event_Management_System.MVC.Models.ViewModels;

namespace Raceday_Event_Management_System.MVC.Services
{
    public class AuthService
    {
        private readonly ApiClient _api;

        public AuthService(ApiClient api)
        {
            _api = api;
        }

        public async Task<ApiResponse<object>?> LoginAsync(LoginViewModel model)
        {
            var endpoint = $"/api/auth/login?email={Uri.EscapeDataString(model.Email)}&password={Uri.EscapeDataString(model.Password)}";
            return await _api.PostAsync<object>(endpoint);
        }

        public async Task<ApiResponse<object>?> RegisterAsync(RegisterViewModel model)
        {
            var endpoint = $"/api/auth/register?" +
                           $"email={Uri.EscapeDataString(model.Email)}" +
                           $"&password={Uri.EscapeDataString(model.Password)}" +
                           $"&confirm_password={Uri.EscapeDataString(model.ConfirmPassword)}" +
                           $"&first_name={Uri.EscapeDataString(model.FirstName)}" +
                           $"&last_name={Uri.EscapeDataString(model.LastName)}" +
                           $"&phone={Uri.EscapeDataString(model.Phone ?? "")}" +
                           $"&role={model.Role}";

            // Only append date if it has a value to prevent API validation errors
            if (model.DateOfBirth.HasValue)
            {
                endpoint += $"&date_of_birth={model.DateOfBirth.Value:yyyy-MM-dd}";
            }

            return await _api.PostAsync<object>(endpoint);
        }

        public async Task<ApiResponse<object>?> LogoutAsync()
        {
            return await _api.PostAsync<object>("/api/auth/logout");
        }

        public async Task<ApiResponse<UserSessionViewModel>?> GetCurrentUserAsync()
        {
            return await _api.GetAsync<UserSessionViewModel>("/api/auth/me");
        }
    }
}