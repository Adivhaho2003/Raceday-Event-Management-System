namespace Raceday_Event_Management_System.MVC.Models.ViewModels
{
    public class MyEnrolmentViewModel
    {
        public int EnrolmentId { get; set; }
        public int EventId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;

        // Map nested objects from the API JSON
        public EventNested Event { get; set; } = new();
        public EventCategoryNested EventCategory { get; set; } = new();
    }

    public class MyResultViewModel
    {
        public int ResultId { get; set; }
        public string? ChipTime { get; set; }
        public int? OverallPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public double? Pace { get; set; }

        // Map nested objects from the API JSON
        public EventNested Event { get; set; } = new();
        public CategoryNested Category { get; set; } = new();
    }

    // Shared Nested Classes
    public class EventNested
    {
        public string EventName { get; set; } = string.Empty;
    }

    public class EventCategoryNested
    {
        public CategoryNested Category { get; set; } = new();
    }

    public class CategoryNested
    {
        public string CategoryName { get; set; } = string.Empty;
    }
}