namespace UnifiedServiceScheduler.Model
{

    public class BookAppointmentRequest
    {
        public long UserId { get; set; }

        public long VehicleId { get; set; }

        public long DealerId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public long BayId { get; set; }

        public long TechnicianId { get; set; }

        public DateTime? StartTimeUtc { get; set; }
    }

}
