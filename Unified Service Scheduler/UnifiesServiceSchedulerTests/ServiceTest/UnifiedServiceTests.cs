using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedServiceScheduler.CustomException;
using UnifiedServiceScheduler.DataAccess;
using UnifiedServiceScheduler.Model;
using UnifiedServiceScheduler.Service;

namespace UnifiesServiceSchedulerTests.ServiceTest
{
    public class UnifiedServiceTests
    {
        private readonly Mock<IUnifiesServiceDataAccess> _dataAccessMock;
        private readonly Mock<ILogger<UnifiedService>> _loggerMock;
        private readonly UnifiedService _service;

        public UnifiedServiceTests()
        {
            _dataAccessMock = new Mock<IUnifiesServiceDataAccess>();
            _loggerMock = new Mock<ILogger<UnifiedService>>();

            _service = new UnifiedService(
                _dataAccessMock.Object,
                _loggerMock.Object);
        }


        [Fact]
        public async Task GetDealerDetail_ReturnsDataFromDataAccess()
        {
            // Arrange
            var search = new DealerSearch();
            var dealers = new List<DealerDetails>();
            long expectedCount = 5;

            _dataAccessMock
                .Setup(x => x.GetDealerDetail(search))
                .ReturnsAsync((expectedCount, dealers));

            // Act
            var result = await _service.GetDealerDetail(search);

            // Assert
            Assert.Equal(expectedCount, result.TotalCount);
            Assert.Same(dealers, result.Dealers);

            _dataAccessMock.Verify(
                x => x.GetDealerDetail(search),
                Times.Once);
        }

        [Fact]
        public async Task GetDealerDetail_PropagatesDataAccessException()
        {
            // Arrange
            var search = new DealerSearch();

            _dataAccessMock
                .Setup(x => x.GetDealerDetail(search))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetDealerDetail(search));
        }


        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetAvailableSlotsAsync_InvalidDealerId_ThrowsException(
            long dealerId)
        {
            // Act
            await Assert.ThrowsAsync<AppointmentBookingException>(
                () => _service.GetAvailableSlotsAsync(
                    dealerId, null, null));

            // Data access should not be called for invalid input.
            _dataAccessMock.Verify(
                x => x.GetAvailableSlotsAsync(
                    It.IsAny<long>(),
                    It.IsAny<string?>(),
                    It.IsAny<DateTime?>()),
                Times.Never);
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_ValidDealer_GroupsSlotsByDealer()
        {
            // Arrange
            long dealerId = 1;
            var startTime = new DateTime(2026, 11, 2, 13, 0, 0);

            var slots = new List<AvailableServiceSlot>
        {
            new()
            {
                DealerId = 1,
                DealerName = "Dealer A"
            },
            new()
            {
                DealerId = 1,
                DealerName = "Dealer A"
            },
            new()
            {
                DealerId = 2,
                DealerName = "Dealer B"
            }
        };

            _dataAccessMock
                .Setup(x => x.GetAvailableSlotsAsync(
                    dealerId, null, startTime))
                .ReturnsAsync(slots);

            // Act
            var result = await _service.GetAvailableSlotsAsync(
                dealerId, null, startTime);

            // Assert
            Assert.Equal(2, result.Count);

            var dealerA = result.Single(x => x.DealerId == 1);
            var dealerB = result.Single(x => x.DealerId == 2);

            Assert.Equal("Dealer A", dealerA.DealerName);
            Assert.Equal(2, dealerA.AvailableSlots.Count);

            Assert.Equal("Dealer B", dealerB.DealerName);
            Assert.Single(dealerB.AvailableSlots);

            _dataAccessMock.Verify(
                x => x.GetAvailableSlotsAsync(
                    dealerId, null, startTime),
                Times.Once);
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_DataAccessThrowsUnexpectedException_Rethrows()
        {
            // Arrange
            _dataAccessMock
                .Setup(x => x.GetAvailableSlotsAsync(
                    1, null, null))
                .ThrowsAsync(new InvalidOperationException("Unexpected error"));

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetAvailableSlotsAsync(1, null, null));
        }


        [Fact]
        public async Task BookAppointmentAsync_ValidRequest_ReturnsAppointmentId()
        {
            // Arrange
            var request = new BookAppointmentRequest
            {
                DealerId = 1,
                ServiceName = "Full Service",
                BayId = 10,
                TechnicianId = 20
            };

            long expectedAppointmentId = 1001;

            _dataAccessMock
                .Setup(x => x.BookAppointmentAsync(request))
                .ReturnsAsync(expectedAppointmentId);

            // Act
            var result = await _service.BookAppointmentAsync(request);

            // Assert
            Assert.Equal(expectedAppointmentId, result);

            _dataAccessMock.Verify(
                x => x.BookAppointmentAsync(request),
                Times.Once);
        }

        [Fact]
        public async Task BookAppointmentAsync_NullRequest_ThrowsException()
        {
            // Act and Assert
            await Assert.ThrowsAsync<AppointmentBookingException>(
                () => _service.BookAppointmentAsync(null!));

            _dataAccessMock.Verify(
                x => x.BookAppointmentAsync(
                    It.IsAny<BookAppointmentRequest>()),
                Times.Never);
        }

        [Theory]
        [InlineData(0, "Full Service", 10, 20)]
        [InlineData(-1, "Full Service", 10, 20)]
        [InlineData(1, "", 10, 20)]
        [InlineData(1, "   ", 10, 20)]
        [InlineData(1, "Full Service", 0, 20)]
        [InlineData(1, "Full Service", 10, 0)]
        public async Task BookAppointmentAsync_InvalidRequest_ThrowsException(
            long dealerId,
            string serviceName,
            long bayId,
            long technicianId)
        {
            // Arrange
            var request = new BookAppointmentRequest
            {
                DealerId = dealerId,
                ServiceName = serviceName,
                BayId = bayId,
                TechnicianId = technicianId
            };

            // Act and Assert
            await Assert.ThrowsAsync<AppointmentBookingException>(
                () => _service.BookAppointmentAsync(request));

            _dataAccessMock.Verify(
                x => x.BookAppointmentAsync(
                    It.IsAny<BookAppointmentRequest>()),
                Times.Never);
        }

        [Fact]
        public async Task BookAppointmentAsync_DataAccessThrowsUnexpectedException_Rethrows()
        {
            // Arrange
            var request = new BookAppointmentRequest
            {
                DealerId = 1,
                ServiceName = "Full Service",
                BayId = 10,
                TechnicianId = 20
            };

            _dataAccessMock
                .Setup(x => x.BookAppointmentAsync(request))
                .ThrowsAsync(new InvalidOperationException("Unexpected error"));

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.BookAppointmentAsync(request));
        }
    }

}
