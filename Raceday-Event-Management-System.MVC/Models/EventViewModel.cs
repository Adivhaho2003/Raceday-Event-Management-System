namespace Raceday_Event_Management_System.MVC.Models
{
    public class EventViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? Distance { get; set; }
        public decimal EntryFee { get; set; }
        public string Status { get; set; } = string.Empty;

        // The image property we just added
        public string? BannerImageUrl { get; set; }
    }
}