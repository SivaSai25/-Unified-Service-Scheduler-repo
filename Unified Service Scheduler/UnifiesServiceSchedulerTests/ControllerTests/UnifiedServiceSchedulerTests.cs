using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedServiceScheduler.Controllers;
using UnifiedServiceScheduler.CustomException;
using UnifiedServiceScheduler.Model;
using UnifiedServiceScheduler.Service;

namespace UnifiesServiceSchedulerTests.ControllerTests
{
    public class UnifiedServiceSchedulerTests
    {
        private readonly Mock<IUnifiedService> _serviceMock;
        private readonly UnifiedServiceSchedulerController _controller;

        public UnifiedServiceSchedulerTests()
        {
            _serviceMock = new Mock<IUnifiedService>();

            _controller = new UnifiedServiceSchedulerController(
                _serviceMock.Object);
        }

        [Fact]
        public async Task GetDealerDetails_ReturnsOk_WhenDealersExist()
        {
            // Arrange
            var search = new DealerSearch
            {
                Limit = 10,
                OffSet = 0
            };

            var dealers = new List<DealerDetails>
            {
                new DealerDetails
                {
                    DealerId = 1,
                    DealerName = "Test Dealer",
                    City = "Mysuru",
                    Address = "Main Road",
                    isActive = true
                }
            };

            _serviceMock
                .Setup(x => x.GetDealerDetail(search))
                .ReturnsAsync((1L, dealers));

            // Act
            var result = await _controller.GetDealerDetails(search);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(200, okResult.StatusCode);

            _serviceMock.Verify(
                x => x.GetDealerDetail(search),
                Times.Once);
        }

        [Fact]
        public async Task GetDealerDetails_ReturnsBadRequest_WhenBookingExceptionOccurs()
        {
            // Arrange
            var search = new DealerSearch();

            _serviceMock
                .Setup(x => x.GetDealerDetail(search))
                .ThrowsAsync(
                    new AppointmentBookingException(1001, "Invalid search"));

            // Act
            var result = await _controller.GetDealerDetails(search);

            // Assert
            var badRequest = Assert.IsType<ObjectResult>(result);

            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public async Task GetDealerDetails_Returns500_WhenUnexpectedExceptionOccurs()
        {
            // Arrange
            var search = new DealerSearch();

            _serviceMock
                .Setup(x => x.GetDealerDetail(search))
                .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _controller.GetDealerDetails(search);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(500, errorResult.StatusCode);
        }


        [Fact]
        public async Task GetAvailableSlots_ReturnsOk_WhenRequestIsValid()
        {
            // Arrange
            long dealerId = 1;
            string serviceName = "Full Service";
            DateTime startTime = new DateTime(2026, 11, 2, 13, 0, 0);

            var slots = new List<DealerAvailableSlots>();

            /*Act*/
            _serviceMock
      .Setup(x => x.GetAvailableSlotsAsync(
          dealerId, serviceName, startTime))
      .ReturnsAsync(slots);

            var result = await _controller.GetAvailableSlots(
                dealerId, serviceName, startTime);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(200, okResult.StatusCode);
            Assert.Same(slots, okResult.Value);
        }

        [Fact]
        public async Task GetAvailableSlots_ReturnsBadRequest_WhenBookingExceptionOccurs()
        {
            // Arrange
            _serviceMock
                .Setup(x => x.GetAvailableSlotsAsync(
                    1, "Invalid Service", null))
                .ThrowsAsync(
                    new AppointmentBookingException(1002, "Service not found"));

            // Act
            var result = await _controller.GetAvailableSlots(
                1, "Invalid Service", null);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(400, errorResult.StatusCode);
        }

        [Fact]
        public async Task GetAvailableSlots_Returns500_WhenUnexpectedExceptionOccurs()
        {
            // Arrange
            _serviceMock
                .Setup(x => x.GetAvailableSlotsAsync(
                    1, null, null))
                .ThrowsAsync(new Exception("Database unavailable"));

            // Act
            var result = await _controller.GetAvailableSlots(
                1, null, null);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(500, errorResult.StatusCode);
        }

     

        [Fact]
        public async Task BookAppointment_ReturnsOk_WhenBookingSucceeds()
        {
            // Arrange
            var request = new BookAppointmentRequest
            {
                UserId = 3,
                VehicleId = 3,
                DealerId = 1,
                ServiceName = "Full Service",
                BayId = 1,
                TechnicianId = 1,
                StartTimeUtc = new DateTime(2026, 11, 2, 13, 0, 0)
            };

            long expectedAppointmentId = 101;

            _serviceMock
                .Setup(x => x.BookAppointmentAsync(request))
                .ReturnsAsync(expectedAppointmentId);

            // Act
            var result = await _controller.BookAppointment(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(200, okResult.StatusCode);

            _serviceMock.Verify(
                x => x.BookAppointmentAsync(request),
                Times.Once);
        }

        [Fact]
        public async Task BookAppointment_ReturnsBadRequest_WhenBookingExceptionOccurs()
        {
            // Arrange
            var request = new BookAppointmentRequest
            {
                UserId = 3,
                VehicleId = 3,
                DealerId = 1,
                ServiceName = "Full Service",
                BayId = 1,
                TechnicianId = 1,
                StartTimeUtc = new DateTime(2026, 11, 2, 13, 0, 0)
            };

            _serviceMock
                .Setup(x => x.BookAppointmentAsync(request))
                .ThrowsAsync(
                    new AppointmentBookingException(
                        1003, "Technician is unavailable"));

            // Act
            var result = await _controller.BookAppointment(request);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(400, errorResult.StatusCode);
        }

        [Fact]
        public async Task BookAppointment_Returns500_WhenUnexpectedExceptionOccurs()
        {
            // Arrange
            var request = new BookAppointmentRequest
            {
                UserId = 3,
                VehicleId = 3,
                DealerId = 1,
                ServiceName = "Full Service",
                BayId = 1,
                TechnicianId = 1,
                StartTimeUtc = new DateTime(2026, 11, 2, 13, 0, 0)
            };

            _serviceMock
                .Setup(x => x.BookAppointmentAsync(request))
                .ThrowsAsync(new Exception("Unexpected database error"));

            // Act
            var result = await _controller.BookAppointment(request);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(500, errorResult.StatusCode);
        }
    }

}
