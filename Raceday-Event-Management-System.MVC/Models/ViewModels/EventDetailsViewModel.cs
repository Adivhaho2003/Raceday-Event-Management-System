namespace Raceday_Event_Management_System.MVC.Models.ViewModels
{
    public class EventDetailsViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? Distance { get; set; }
        public string Status { get; set; } = string.Empty;

        // Add this property
        public string? BannerImageUrl { get; set; }

        public List<EventCategoryItem> Categories { get; set; } = new();

        public bool IsAlreadyEnrolled { get; set; }
    }

    public class EventCategoryItem
    {
        public int EventCategoryId { get; set; }
        public decimal Fee { get; set; }
        public int? Capacity { get; set; }

        public CategoryViewModel? Category { get; set; }

        public string CategoryName => Category?.CategoryName ?? "Unknown Category";
    }
}