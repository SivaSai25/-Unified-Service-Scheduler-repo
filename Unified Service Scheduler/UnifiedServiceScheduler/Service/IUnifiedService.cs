using UnifiedServiceScheduler.Model;

namespace UnifiedServiceScheduler.Service
{
    public interface IUnifiedService
    {
        Task<(long TotalCount, List<DealerDetails> Dealers)> GetDealerDetail(DealerSearch dealerSearch);
        Task<List<DealerAvailableSlots>> GetAvailableSlotsAsync(long dealerId, string? serviceName, DateTime? startTime);

        Task<long> BookAppointmentAsync(BookAppointmentRequest request);

       // Task<> GetAppointmentDetails(int userId);
    }
}
