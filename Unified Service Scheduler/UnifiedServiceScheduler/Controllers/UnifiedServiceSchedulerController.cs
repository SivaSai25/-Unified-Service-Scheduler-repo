using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnifiedServiceScheduler.CustomException;
using UnifiedServiceScheduler.Model;
using UnifiedServiceScheduler.Service;

namespace UnifiedServiceScheduler.Controllers
{
    [ApiController]
    [Route("api/UnifiedServiceScheduler")]
    public class UnifiedServiceSchedulerController : Controller
    {
        private readonly IUnifiedService _unifiedService;
        public UnifiedServiceSchedulerController(IUnifiedService unifiedService)
        {
            _unifiedService = unifiedService;
        }

        /// <summary>
        /// Get Dealer Details
        /// </summary>
        /// <param name="dealerSearch"></param>
        /// <returns></returns>

        [HttpPost("getDealerDetails")]
        public async Task<IActionResult> GetDealerDetails(DealerSearch dealerSearch)
        {

            try
            {
                var result = await _unifiedService.GetDealerDetail(dealerSearch);
                var pageHandler = new Paging
                {
                    OffSet = dealerSearch.OffSet,
                    Limit = dealerSearch.Limit,
                    Total = result.TotalCount,
                    ReturnedCount = result.Dealers.Count()

                };
                var response = new DealerDetailsResponseViewModel
                {
                    DealerDetails = result.Dealers,
                    Paging = pageHandler,

                };
                return Ok(response);
            }
            catch (AppointmentBookingException ex)
            {
                return StatusCode(400, new
                {
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message
                });
            }
            catch (Exception)
            {
                // Log the original exception using your application's logger.
                return StatusCode(500, new
                {
                    ErrorCode = Constants.InternalServerError,
                    Message = "An unexpected error occurred"
                });
            }
        }

        /// <summary>
        /// Get available slots of bay and technician of selected bay
        /// </summary>
        /// <param name="dealerId"></param>
        /// <param name="serviceName"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        [HttpGet("getAvailableSlots")]
        public async Task<IActionResult> GetAvailableSlots(
    [FromQuery] long dealerId,
    [FromQuery] string? serviceName,
    [FromQuery] DateTime? startTime)
        {
            try
            {
                var slots = await _unifiedService.GetAvailableSlotsAsync(
                    dealerId, serviceName, startTime);

                return Ok(slots);
            }
            catch (AppointmentBookingException ex)
            {
                return StatusCode(400, new
                {
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message
                });
            }
            catch (Exception)
            {
                // Log the original exception using your application's logger.
                return StatusCode(500, new
                {
                    ErrorCode = Constants.InternalServerError,
                    Message = "An unexpected error occurred"
                });
            }
        }

        /// <summary>
        /// Book an Appointment for service
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost("bookAppointment")]
        public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequest request)
        {
            try
            {
                var appointmentId = await _unifiedService.BookAppointmentAsync(request);

                return Ok(new { AppointmentId = appointmentId });
            }
            catch (AppointmentBookingException ex)
            {
                return StatusCode(400, new
                {
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message
                });
            }
            catch (Exception)
            {
                // Log the original exception using your application's logger.
                return StatusCode(500, new
                {
                    ErrorCode = Constants.InternalServerError,
                    Message = "An unexpected error occurred while booking the appointment."
                });
            }
        }
    }
}
