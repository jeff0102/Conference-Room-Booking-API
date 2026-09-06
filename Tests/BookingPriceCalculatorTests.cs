using ConferenceRoomBookingApi.Application.Services;
using ConferenceRoomBookingApi.Domain.Entities;
using Xunit;

namespace ConferenceRoomBookingApi.Tests;

public class BookingPriceCalculatorTests
{
    private readonly BookingPriceCalculator _calculator = new();
    private readonly DateTime _testDate = new(2026, 9, 10); // Fixed arbitrary date

    #region Single Interval Tests

    [Fact]
    public void CalculateRoomPrice_MorningHours_Applies10PercentDiscount()
    {
        // Arrange: 07:00 to 09:00 (2 hours in Morning band: 06:00 - 09:00)
        var startTime = _testDate.AddHours(7);
        const int durationHours = 2;
        const decimal basePricePerHour = 2000m; // Room A rate

        // Expected: 2 hours * (2000 * 0.90) = 3600
        const decimal expected = 3600m;

        // Act
        var result = _calculator.CalculateRoomPrice(basePricePerHour, startTime, durationHours);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateRoomPrice_StandardMorningHours_AppliesBaseRate()
    {
        // Arrange: 09:00 to 12:00 (3 hours in Standard band: 09:00 - 12:00)
        var startTime = _testDate.AddHours(9);
        const int durationHours = 3;
        const decimal basePricePerHour = 2000m;

        // Expected: 3 hours * (2000 * 1.00) = 6000
        const decimal expected = 6000m;

        // Act
        var result = _calculator.CalculateRoomPrice(basePricePerHour, startTime, durationHours);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateRoomPrice_PeakHours_Applies15PercentSurcharge()
    {
        // Arrange: 12:00 to 14:00 (2 hours in Peak band: 12:00 - 14:00)
        var startTime = _testDate.AddHours(12);
        const int durationHours = 2;
        const decimal basePricePerHour = 2000m;

        // Expected: 2 hours * (2000 * 1.15) = 4600
        const decimal expected = 4600m;

        // Act
        var result = _calculator.CalculateRoomPrice(basePricePerHour, startTime, durationHours);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateRoomPrice_StandardAfternoonHours_AppliesBaseRate()
    {
        // Arrange: 14:00 to 18:00 (4 hours in Standard band: 14:00 - 18:00)
        var startTime = _testDate.AddHours(14);
        const int durationHours = 4;
        const decimal basePricePerHour = 3500m; // Room B rate

        // Expected: 4 hours * (3500 * 1.00) = 14000
        const decimal expected = 14000m;

        // Act
        var result = _calculator.CalculateRoomPrice(basePricePerHour, startTime, durationHours);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateRoomPrice_EveningHours_Applies20PercentDiscount()
    {
        // Arrange: 18:00 to 22:00 (4 hours in Evening band: 18:00 - 23:00)
        var startTime = _testDate.AddHours(18);
        const int durationHours = 4;
        const decimal basePricePerHour = 1500m; // Room C rate

        // Expected: 4 hours * (1500 * 0.80) = 4800
        const decimal expected = 4800m;

        // Act
        var result = _calculator.CalculateRoomPrice(basePricePerHour, startTime, durationHours);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region Spanning Multiple Intervals Tests

    [Fact]
    public void CalculateRoomPrice_SpanningMorningAndStandard_CalculatesHourlyProrated()
    {
        // Arrange: 08:00 to 10:00 (2 hours)
        // 08:00 - 09:00: 1 hour Morning (0.90 * 2000 = 1800)
        // 09:00 - 10:00: 1 hour Standard (1.00 * 2000 = 2000)
        // Total expected: 1800 + 2000 = 3800
        var startTime = _testDate.AddHours(8);
        const int durationHours = 2;
        const decimal basePricePerHour = 2000m;
        const decimal expected = 3800m;

        // Act
        var result = _calculator.CalculateRoomPrice(basePricePerHour, startTime, durationHours);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateRoomPrice_SpanningStandardAndPeak_CalculatesHourlyProrated()
    {
        // Arrange: 11:00 to 13:00 (2 hours)
        // 11:00 - 12:00: 1 hour Standard (1.00 * 2000 = 2000)
        // 12:00 - 13:00: 1 hour Peak (1.15 * 2000 = 2300)
        // Total expected: 2000 + 2300 = 4300
        var startTime = _testDate.AddHours(11);
        const int durationHours = 2;
        const decimal basePricePerHour = 2000m;
        const decimal expected = 4300m;

        // Act
        var result = _calculator.CalculateRoomPrice(basePricePerHour, startTime, durationHours);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateRoomPrice_SpanningPeakAndStandard_CalculatesHourlyProrated()
    {
        // Arrange: 13:00 to 15:00 (2 hours)
        // 13:00 - 14:00: 1 hour Peak (1.15 * 2000 = 2300)
        // 14:00 - 15:00: 1 hour Standard (1.00 * 2000 = 2000)
        // Total expected: 2300 + 2000 = 4300
        var startTime = _testDate.AddHours(13);
        const int durationHours = 2;
        const decimal basePricePerHour = 2000m;
        const decimal expected = 4300m;

        // Act
        var result = _calculator.CalculateRoomPrice(basePricePerHour, startTime, durationHours);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateRoomPrice_SpanningStandardThroughPeakToStandard_CalculatesHourlyProrated()
    {
        // Arrange: 11:00 to 15:00 (4 hours)
        // 11:00 - 12:00: 1 hour Standard (1.00 * 2000 = 2000)
        // 12:00 - 14:00: 2 hours Peak (2 * 1.15 * 2000 = 4600)
        // 14:00 - 15:00: 1 hour Standard (1.00 * 2000 = 2000)
        // Total expected: 2000 + 4600 + 2000 = 8600
        var startTime = _testDate.AddHours(11);
        const int durationHours = 4;
        const decimal basePricePerHour = 2000m;
        const decimal expected = 8600m;

        // Act
        var result = _calculator.CalculateRoomPrice(basePricePerHour, startTime, durationHours);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateRoomPrice_SpanningStandardAndEvening_CalculatesHourlyProrated()
    {
        // Arrange: 17:00 to 19:00 (2 hours)
        // 17:00 - 18:00: 1 hour Standard (1.00 * 2000 = 2000)
        // 18:00 - 19:00: 1 hour Evening (0.80 * 2000 = 1600)
        // Total expected: 2000 + 1600 = 3600
        var startTime = _testDate.AddHours(17);
        const int durationHours = 2;
        const decimal basePricePerHour = 2000m;
        const decimal expected = 3600m;

        // Act
        var result = _calculator.CalculateRoomPrice(basePricePerHour, startTime, durationHours);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateRoomPrice_SpanningEntireWorkDay_CalculatesAllIntervalsCorrectly()
    {
        // Arrange: 08:00 to 20:00 (12 hours)
        // 08:00 - 09:00: 1 hr Morning (0.90 * 2000 = 1800)
        // 09:00 - 12:00: 3 hrs Standard (3 * 2000 = 6000)
        // 12:00 - 14:00: 2 hrs Peak (2 * 1.15 * 2000 = 4600)
        // 14:00 - 18:00: 4 hrs Standard (4 * 2000 = 8000)
        // 18:00 - 20:00: 2 hrs Evening (2 * 0.80 * 2000 = 3200)
        // Total expected: 1800 + 6000 + 4600 + 8000 + 3200 = 23600
        var startTime = _testDate.AddHours(8);
        const int durationHours = 12;
        const decimal basePricePerHour = 2000m;
        const decimal expected = 23600m;

        // Act
        var result = _calculator.CalculateRoomPrice(basePricePerHour, startTime, durationHours);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateRoomPrice_ProratedStartAtHalfHour_CalculatesMinuteByMinuteAccurately()
    {
        // Arrange: 08:30 to 09:30 (1 hour)
        // 08:30 - 09:00 (30 mins Morning @ 0.90): 30 * 0.90 = 27 weighted minutes
        // 09:00 - 09:30 (30 mins Standard @ 1.00): 30 * 1.00 = 30 weighted minutes
        // Total weighted minutes: 57
        // Room Price = (2000 * 57) / 60 = 1900.00
        var startTime = _testDate.AddHours(8).AddMinutes(30);
        const int durationHours = 1;
        const decimal basePricePerHour = 2000m;
        const decimal expected = 1900.00m;

        // Act
        var result = _calculator.CalculateRoomPrice(basePricePerHour, startTime, durationHours);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region Total Price with Services Tests

    [Fact]
    public void CalculateTotalPrice_WithServices_AddsServicesToRoomPrice()
    {
        // Arrange: Room A from 10:00 to 12:00 (2 hours Standard = 4000)
        // Services: Projector (500) + Sound (700) + Wi-Fi (300) = 1500
        // Total expected: 4000 + 1500 = 5500
        var startTime = _testDate.AddHours(10);
        const int durationHours = 2;
        const decimal basePricePerHour = 2000m;

        var services = new List<Service>
        {
            new(1, "Projector", 500m),
            new(2, "Wi-Fi", 300m),
            new(3, "Sound", 700m)
        };

        const decimal expected = 5500m;

        // Act
        var result = _calculator.CalculateTotalPrice(basePricePerHour, startTime, durationHours, services);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateTotalPrice_WithoutServices_ReturnsOnlyRoomPrice()
    {
        // Arrange: Room A from 06:00 to 09:00 (3 hours Morning = 3 * 1800 = 5400)
        var startTime = _testDate.AddHours(6);
        const int durationHours = 3;
        const decimal basePricePerHour = 2000m;
        const decimal expected = 5400m;

        // Act
        var result = _calculator.CalculateTotalPrice(basePricePerHour, startTime, durationHours, null);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void BookingEntity_CalculateTotalPrice_UsesInjectedCalculator()
    {
        // Arrange
        var booking = new Booking
        {
            RoomId = 1,
            BookingDate = _testDate.AddHours(12), // 12:00 Peak start
            DurationHours = 2,                    // 2 hrs Peak (1.15 * 2000 * 2 = 4600)
            SelectedServices = new List<Service>
            {
                new(1, "Projector", 500m)
            }
        };

        // Act
        var totalPrice = booking.CalculateTotalPrice(2000m, _calculator);

        // Assert: 4600 (room) + 500 (service) = 5100
        Assert.Equal(5100m, totalPrice);
        Assert.Equal(5100m, booking.TotalPrice);
    }

    #endregion

    #region Rate Multiplier Boundaries & Validation Tests

    [Theory]
    [InlineData(6, 0, 0.90)]   // 06:00 - Morning Start
    [InlineData(7, 30, 0.90)]  // 07:30 - Morning Middle
    [InlineData(8, 59, 0.90)]  // 08:59 - Morning End
    [InlineData(9, 0, 1.00)]   // 09:00 - Standard Start
    [InlineData(11, 59, 1.00)] // 11:59 - Standard End
    [InlineData(12, 0, 1.15)]  // 12:00 - Peak Start
    [InlineData(13, 30, 1.15)] // 13:30 - Peak Middle
    [InlineData(13, 59, 1.15)] // 13:59 - Peak End
    [InlineData(14, 0, 1.00)]  // 14:00 - Standard Afternoon Start
    [InlineData(17, 59, 1.00)] // 17:59 - Standard Afternoon End
    [InlineData(18, 0, 0.80)]  // 18:00 - Evening Start
    [InlineData(22, 59, 0.80)] // 22:59 - Evening End
    [InlineData(23, 0, 1.00)]  // 23:00 - Outside evening
    public void GetRateMultiplier_ReturnsExpectedMultiplier(int hour, int minute, decimal expectedMultiplier)
    {
        // Act
        var multiplier = _calculator.GetRateMultiplier(new TimeSpan(hour, minute, 0));

        // Assert
        Assert.Equal(expectedMultiplier, multiplier);
    }

    [Fact]
    public void CalculateRoomPrice_NegativeBasePrice_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculator.CalculateRoomPrice(-100m, _testDate.AddHours(10), 2));
    }

    [Fact]
    public void CalculateRoomPrice_ZeroOrNegativeDuration_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculator.CalculateRoomPrice(2000m, _testDate.AddHours(10), 0));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculator.CalculateRoomPrice(2000m, _testDate.AddHours(10), -2));
    }

    #endregion
}
