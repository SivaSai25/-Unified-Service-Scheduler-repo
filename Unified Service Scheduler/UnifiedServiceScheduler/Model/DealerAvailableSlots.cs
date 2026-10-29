namespace UnifiedServiceScheduler.Model
{
    public class DealerAvailableSlots
    {
       public long DealerId { get; set; }
        public string DealerName { get; set; } = string.Empty;

        public List<AvailableServiceSlot> AvailableSlots { get; set; } = new();
    }
}
