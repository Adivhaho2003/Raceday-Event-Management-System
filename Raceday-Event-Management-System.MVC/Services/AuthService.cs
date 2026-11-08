namespace Raceday_Event_Management_System.MVC.Services
{
    public class AuthService
    {
        private readonly ApiClient _api;

        public AuthService(ApiClient api)
        {
            _api = api;
        }
    }
}