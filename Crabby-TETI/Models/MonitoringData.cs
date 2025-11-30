using System;

namespace CrabbyTETI.Models
{
    // Monitoring data model
    public class MonitoringData
    {
        public int Id { get; set; }
        public int TambakId { get; set; }
        public decimal Suhu { get; set; }
        public decimal Salinitas { get; set; }
        public decimal Ph { get; set; }
        public decimal? Oksigen { get; set; }
        public DateTime RecordedAt { get; set; }

        // Formatted time labels
        public string TimeLabel => RecordedAt.ToString("HH:mm");
        public string DateTimeLabel => RecordedAt.ToString("dd/MM HH:mm");
    }
}
