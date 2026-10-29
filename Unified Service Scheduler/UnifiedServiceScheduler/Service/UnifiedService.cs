using Microsoft.Data.SqlClient;
using UnifiedServiceScheduler.CustomException;
using UnifiedServiceScheduler.DataAccess;
using UnifiedServiceScheduler.Model;

namespace UnifiedServiceScheduler.Service
{
    public class UnifiedService : IUnifiedService
    {
        private readonly IUnifiesServiceDataAccess _dataAccess;
        private readonly ILogger<UnifiedService> _logger;

        public UnifiedService(IUnifiesServiceDataAccess dataAccess,ILogger<UnifiedService> logger)
        {
            _dataAccess = dataAccess;
            _logger = logger;
        }
     

        public async Task<(long TotalCount, List<DealerDetails> Dealers)> GetDealerDetail(DealerSearch dealerSearch)
        {
            
                return await _dataAccess.GetDealerDetail(dealerSearch);
            
        }

        public async Task<List<DealerAvailableSlots>> GetAvailableSlotsAsync(long dealerId,string? serviceName,DateTime? startTime)
        {

                if (dealerId <= 0)
                {
                    _logger.LogWarning("GetAvailableSlotsAsync booking rejected due to invalid input.");

                    throw new AppointmentBookingException(Constants.InvalidInput, Constants.InvalidInputData);
                }
            try
            {

                var slots = await _dataAccess.GetAvailableSlotsAsync(
                    dealerId,
                    serviceName,
                    startTime);

                var result = slots
                    .GroupBy(x => new { x.DealerId, x.DealerName })
                    .Select(group => new DealerAvailableSlots
                    {
                        DealerId = group.Key.DealerId,
                        DealerName = group.Key.DealerName,
                        AvailableSlots = group.ToList()
                    })
                    .ToList();

                return result;
            }
            catch (SqlException ex) when (ex.Number >= 50001 && ex.Number <= 50009)
            {
                _logger.LogWarning(" Get appitemnet slots failed with SQL business error {SqlErrorCode}", ex.Number);
                throw new AppointmentBookingException(
                        ex.Number,
                        ex.Message
                        );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error during get slots booking.");

                throw;
            }


        }

        public async Task<long> BookAppointmentAsync(BookAppointmentRequest request)
        {
            try
            {
                if (request == null || request.DealerId <= 0 || string.IsNullOrWhiteSpace(request.ServiceName) || request.BayId <= 0 || request.TechnicianId <= 0)
                {
                    _logger.LogWarning("Appointment booking rejected due to invalid input.");
                    throw new AppointmentBookingException(Constants.InvalidInput, Constants.InvalidInputData);
                 }
                _logger.LogInformation("Appointment booking started for DealerId {DealerId}, " +"BayId {BayId}, TechnicianId {TechnicianId}",request.DealerId,request.BayId,request.TechnicianId);

                long appointmentId = await _dataAccess.BookAppointmentAsync(request);
                _logger.LogInformation("Appointment booking succeeded. AppointmentId {AppointmentId}",appointmentId);
                return appointmentId;

            }
            catch (SqlException ex)
            {
            _logger.LogWarning("Appointment booking failed with SQL business error {SqlErrorCode}",ex.Number);
            throw new AppointmentBookingException(
                    ex.Number,
                    ex.Message
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error during appointment booking.");

                throw;
            }


        }
    }
}
