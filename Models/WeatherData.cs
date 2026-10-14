using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models
{
    public class WeatherData
    {
        public int WeatherId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public DateTime ForecastDate { get; set; }

        public TimeSpan? ForecastTime { get; set; }

        public double? Temperature { get; set; }

        public double? Humidity { get; set; }

        public double? WindSpeed { get; set; }

        [StringLength(50)]
        public string? WindDirection { get; set; }

        [StringLength(255)]
        public string? Conditions { get; set; }

        [StringLength(100)]
        public string? Source { get; set; }

        public DateTime RetrievedAt { get; set; } = DateTime.Now;

        // Navigation property
        public Event? Event { get; set; }
    }
}