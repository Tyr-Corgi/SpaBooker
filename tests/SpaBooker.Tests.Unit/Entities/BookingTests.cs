using FluentAssertions;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;

namespace SpaBooker.Tests.Unit.Entities;

public class BookingTests
{
    [Fact]
    public void Booking_Should_Create_With_Valid_Properties()
    {
        // Arrange
        var clientId = "user-123";
        var therapistId = "therapist-456";
        var serviceId = 1;
        var locationId = 2;
        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(1);
        var totalPrice = 100.00m;

        // Act
        var booking = new Booking
        {
            ClientId = clientId,
            TherapistId = therapistId,
            ServiceId = serviceId,
            LocationId = locationId,
            StartTime = startTime,
            EndTime = endTime,
            Status = BookingStatus.Pending,
            TotalPrice = totalPrice,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        booking.ClientId.Should().Be(clientId);
        booking.TherapistId.Should().Be(therapistId);
        booking.ServiceId.Should().Be(serviceId);
        booking.LocationId.Should().Be(locationId);
        booking.StartTime.Should().Be(startTime);
        booking.EndTime.Should().Be(endTime);
        booking.Status.Should().Be(BookingStatus.Pending);
        booking.TotalPrice.Should().Be(totalPrice);
    }

    [Fact]
    public void Booking_Duration_Should_Be_Correct()
    {
        // Arrange
        var startTime = new DateTime(2025, 11, 15, 10, 0, 0, DateTimeKind.Utc);
        var endTime = new DateTime(2025, 11, 15, 11, 30, 0, DateTimeKind.Utc);
        
        var booking = new Booking
        {
            StartTime = startTime,
            EndTime = endTime
        };

        // Act
        var duration = booking.EndTime - booking.StartTime;

        // Assert
        duration.TotalMinutes.Should().Be(90);
    }

    [Fact]
    public void Booking_Status_Should_Be_Changeable()
    {
        // Arrange
        var booking = new Booking
        {
            Status = BookingStatus.Pending
        };

        // Act
        booking.Status = BookingStatus.Confirmed;

        // Assert
        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Completed)]
    [InlineData(BookingStatus.NoShow)]
    public void Booking_Should_Accept_All_Valid_Statuses(BookingStatus status)
    {
        // Arrange & Act
        var booking = new Booking
        {
            Status = status
        };

        // Assert
        booking.Status.Should().Be(status);
    }

    [Fact]
    public void Booking_Notes_Should_Be_Optional()
    {
        // Arrange & Act
        var booking = new Booking
        {
            Notes = null
        };

        // Assert
        booking.Notes.Should().BeNull();
    }

    [Fact]
    public void Booking_With_Notes_Should_Store_Notes()
    {
        // Arrange
        var notes = "Client prefers deep tissue massage";

        // Act
        var booking = new Booking
        {
            Notes = notes
        };

        // Assert
        booking.Notes.Should().Be(notes);
    }

    [Fact]
    public void Booking_Should_Track_Membership_Credits_Usage()
    {
        // Arrange & Act
        var booking = new Booking
        {
            UsedMembershipCredits = true,
            CreditsUsed = 1.0m
        };

        // Assert
        booking.UsedMembershipCredits.Should().BeTrue();
        booking.CreditsUsed.Should().Be(1.0m);
    }

    [Fact]
    public void Booking_Should_Track_Payment_Details()
    {
        // Arrange
        var servicePrice = 120.00m;
        var depositAmount = 30.00m;
        var discountApplied = 10.00m;
        var totalPrice = 110.00m;

        // Act
        var booking = new Booking
        {
            ServicePrice = servicePrice,
            DepositAmount = depositAmount,
            DiscountApplied = discountApplied,
            TotalPrice = totalPrice
        };

        // Assert
        booking.ServicePrice.Should().Be(servicePrice);
        booking.DepositAmount.Should().Be(depositAmount);
        booking.DiscountApplied.Should().Be(discountApplied);
        booking.TotalPrice.Should().Be(totalPrice);
    }
}
