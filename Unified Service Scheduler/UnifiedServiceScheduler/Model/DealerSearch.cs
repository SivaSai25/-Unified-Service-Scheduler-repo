namespace UnifiedServiceScheduler.Model
{
    public class DealerSearch
    {
        public int Limit { get; set; } = 50;

        public int OffSet { get; set; } = 0;

        public string DealerName { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string ServiceType { get; set; } = string.Empty;

        public string SortOrder { get; set; } = "ASC";


    }
}
