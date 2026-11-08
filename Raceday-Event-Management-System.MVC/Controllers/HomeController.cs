using Microsoft.AspNetCore.Mvc;
using Raceday_Event_Management_System.MVC.Services;

namespace Raceday_Event_Management_System.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly EventService _eventService;

        public HomeController(EventService eventService)
        {
            _eventService = eventService;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _eventService.GetAllEventsAsync();

            // Show the 3 most recent upcoming events on the home page
            var upcomingEvents = events
                .Where(e => e.EventDate >= DateTime.Now)
                .OrderBy(e => e.EventDate)
                .Take(3)
                .ToList();

            return View(upcomingEvents);
        }
    }
}