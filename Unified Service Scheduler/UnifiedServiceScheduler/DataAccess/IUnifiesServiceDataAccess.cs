using UnifiedServiceScheduler.Model;

namespace UnifiedServiceScheduler.DataAccess
{
    public interface IUnifiesServiceDataAccess
    {
        Task<(long TotalCount, List<DealerDetails> Dealers)> GetDealerDetail(DealerSearch dealerSearch);

        Task<List<AvailableServiceSlot>> GetAvailableSlotsAsync(long dealerId, string? serviceName, DateTime? startTime);

        Task<long> BookAppointmentAsync(BookAppointmentRequest request);
    }
}
