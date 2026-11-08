namespace Raceday_Event_Management_System.MVC.Services
{
    public class EventService
    {
        private readonly ApiClient _api;

        public EventService(ApiClient api)
        {
            _api = api;
        }

        public async Task<int> GetEventCountAsync()
        {
            var response = await _api.GetAsync<List<object>>("/api/events");
            if (response?.IsSuccess == true && response.Data != null)
            {
                return response.Data.Count;
            }
            return -1;
        }
    }
}