namespace UnifiedServiceScheduler.Model
{
    public class AvailableServiceSlot
    {
        public long DealerId { get; set; }
        public string DealerName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;

        public DateTime? StartTimeUtc { get; set; }
        public DateTime? EndTimeUtc { get; set; }

        public long BayId { get; set; }
        public string BayName { get; set; } = string.Empty;

        public long TechnicianId { get; set; }
        public string TechnicianName { get; set; } = string.Empty;

        public bool? IsBayAvailable { get; set; }
       
        public bool? IsTechnicianAvailable { get; set; }

    }
}
