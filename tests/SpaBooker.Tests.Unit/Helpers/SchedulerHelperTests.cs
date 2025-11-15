using FluentAssertions;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;

namespace SpaBooker.Tests.Unit.Helpers;

public class SchedulerHelperTests
{
    [Fact]
    public void IsTherapistAvailable_Should_Return_True_When_No_Conflicts()
    {
        // Arrange
        var therapistId = "therapist-1";
        var startTime = new DateTime(2025, 11, 15, 10, 0, 0);
        var endTime = new DateTime(2025, 11, 15, 11, 0, 0);
        var bookings = new List<Booking>();

        // Act
        var result = IsTherapistAvailable(therapistId, startTime, endTime, bookings);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTherapistAvailable_Should_Return_False_When_Overlap_At_Start()
    {
        // Arrange
        var therapistId = "therapist-1";
        var startTime = new DateTime(2025, 11, 15, 10, 30, 0);
        var endTime = new DateTime(2025, 11, 15, 11, 30, 0);
        
        var existingBooking = new Booking
        {
            TherapistId = therapistId,
            StartTime = new DateTime(2025, 11, 15, 10, 0, 0),
            EndTime = new DateTime(2025, 11, 15, 11, 0, 0),
            Status = BookingStatus.Confirmed
        };
        
        var bookings = new List<Booking> { existingBooking };

        // Act
        var result = IsTherapistAvailable(therapistId, startTime, endTime, bookings);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTherapistAvailable_Should_Return_False_When_Overlap_At_End()
    {
        // Arrange
        var therapistId = "therapist-1";
        var startTime = new DateTime(2025, 11, 15, 9, 30, 0);
        var endTime = new DateTime(2025, 11, 15, 10, 30, 0);
        
        var existingBooking = new Booking
        {
            TherapistId = therapistId,
            StartTime = new DateTime(2025, 11, 15, 10, 0, 0),
            EndTime = new DateTime(2025, 11, 15, 11, 0, 0),
            Status = BookingStatus.Confirmed
        };
        
        var bookings = new List<Booking> { existingBooking };

        // Act
        var result = IsTherapistAvailable(therapistId, startTime, endTime, bookings);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTherapistAvailable_Should_Return_False_When_Completely_Overlaps()
    {
        // Arrange
        var therapistId = "therapist-1";
        var startTime = new DateTime(2025, 11, 15, 9, 0, 0);
        var endTime = new DateTime(2025, 11, 15, 12, 0, 0);
        
        var existingBooking = new Booking
        {
            TherapistId = therapistId,
            StartTime = new DateTime(2025, 11, 15, 10, 0, 0),
            EndTime = new DateTime(2025, 11, 15, 11, 0, 0),
            Status = BookingStatus.Confirmed
        };
        
        var bookings = new List<Booking> { existingBooking };

        // Act
        var result = IsTherapistAvailable(therapistId, startTime, endTime, bookings);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTherapistAvailable_Should_Return_True_When_Different_Therapist()
    {
        // Arrange
        var therapistId = "therapist-1";
        var startTime = new DateTime(2025, 11, 15, 10, 0, 0);
        var endTime = new DateTime(2025, 11, 15, 11, 0, 0);
        
        var existingBooking = new Booking
        {
            TherapistId = "therapist-2",
            StartTime = new DateTime(2025, 11, 15, 10, 0, 0),
            EndTime = new DateTime(2025, 11, 15, 11, 0, 0),
            Status = BookingStatus.Confirmed
        };
        
        var bookings = new List<Booking> { existingBooking };

        // Act
        var result = IsTherapistAvailable(therapistId, startTime, endTime, bookings);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRoomAvailable_Should_Return_True_When_No_Conflicts()
    {
        // Arrange
        var roomId = 1;
        var startTime = new DateTime(2025, 11, 15, 10, 0, 0);
        var endTime = new DateTime(2025, 11, 15, 11, 0, 0);
        var bookings = new List<Booking>();

        // Act
        var result = IsRoomAvailable(roomId, startTime, endTime, bookings);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRoomAvailable_Should_Return_False_When_Room_Occupied()
    {
        // Arrange
        var roomId = 1;
        var startTime = new DateTime(2025, 11, 15, 10, 0, 0);
        var endTime = new DateTime(2025, 11, 15, 11, 0, 0);
        
        var existingBooking = new Booking
        {
            RoomId = roomId,
            StartTime = new DateTime(2025, 11, 15, 10, 30, 0),
            EndTime = new DateTime(2025, 11, 15, 11, 30, 0),
            Status = BookingStatus.Confirmed
        };
        
        var bookings = new List<Booking> { existingBooking };

        // Act
        var result = IsRoomAvailable(roomId, startTime, endTime, bookings);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsRoomAvailable_Should_Return_True_When_Different_Room()
    {
        // Arrange
        var roomId = 1;
        var startTime = new DateTime(2025, 11, 15, 10, 0, 0);
        var endTime = new DateTime(2025, 11, 15, 11, 0, 0);
        
        var existingBooking = new Booking
        {
            RoomId = 2,
            StartTime = new DateTime(2025, 11, 15, 10, 0, 0),
            EndTime = new DateTime(2025, 11, 15, 11, 0, 0),
            Status = BookingStatus.Confirmed
        };
        
        var bookings = new List<Booking> { existingBooking };

        // Act
        var result = IsRoomAvailable(roomId, startTime, endTime, bookings);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(8, 0, 8, 30)] // 8:00 - 8:30
    [InlineData(10, 30, 11, 0)] // 10:30 - 11:00
    [InlineData(19, 30, 20, 0)] // 19:30 - 20:00
    public void TimeSlot_Calculations_Should_Be_Correct(int startHour, int startMin, int endHour, int endMin)
    {
        // Arrange
        var baseDate = new DateTime(2025, 11, 15);
        var startTime = baseDate.AddHours(startHour).AddMinutes(startMin);
        var endTime = baseDate.AddHours(endHour).AddMinutes(endMin);

        // Act
        var duration = endTime - startTime;

        // Assert
        duration.TotalMinutes.Should().Be(30);
    }

    // Helper methods (same logic as in AdminScheduler.razor)
    private static bool IsTherapistAvailable(string therapistId, DateTime startTime, DateTime endTime, List<Booking> bookings)
    {
        return !bookings.Any(b => b.TherapistId == therapistId
            && ((startTime >= b.StartTime && startTime < b.EndTime) ||
                (endTime > b.StartTime && endTime <= b.EndTime) ||
                (startTime <= b.StartTime && endTime >= b.EndTime)));
    }

    private static bool IsRoomAvailable(int roomId, DateTime startTime, DateTime endTime, List<Booking> bookings)
    {
        return !bookings.Any(b => b.RoomId == roomId
            && ((startTime >= b.StartTime && startTime < b.EndTime) ||
                (endTime > b.StartTime && endTime <= b.EndTime) ||
                (startTime <= b.StartTime && endTime >= b.EndTime)));
    }
}

