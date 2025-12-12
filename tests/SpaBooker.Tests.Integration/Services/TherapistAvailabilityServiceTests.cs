using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;
using SpaBooker.Core.Settings;
using SpaBooker.Infrastructure.Data;
using SpaBooker.Infrastructure.Services;

namespace SpaBooker.Tests.Integration.Services;

public class TherapistAvailabilityServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly TherapistAvailabilityService _service;

    public TherapistAvailabilityServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var bufferSettings = new BufferTimeSettings { BufferMinutes = 15 };
        var optionsWrapper = Options.Create(bufferSettings);

        _service = new TherapistAvailabilityService(_context, optionsWrapper);
    }

    #region IsTherapistAvailableAsync Tests

    [Fact]
    public async Task IsTherapistAvailableAsync_Should_Return_False_When_Therapist_Not_Found()
    {
        // Act
        var result = await _service.IsTherapistAvailableAsync(
            "non-existent-therapist",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1)
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Therapist.NotFound");
    }

    [Fact]
    public async Task IsTherapistAvailableAsync_Should_Return_False_When_No_Availability_Record()
    {
        // Arrange
        var therapist = new ApplicationUser 
        { 
            Id = "therapist-1", 
            UserName = "therapist", 
            Email = "therapist@test.com" 
        };

        await _context.Users.AddAsync(therapist);
        await _context.SaveChangesAsync();

        var requestTime = new DateTime(2025, 12, 15, 10, 0, 0); // Monday

        // Act
        var result = await _service.IsTherapistAvailableAsync(
            therapist.Id,
            requestTime,
            requestTime.AddHours(1)
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsTherapistAvailableAsync_Should_Return_True_When_Available()
    {
        // Arrange
        var therapist = new ApplicationUser 
        { 
            Id = "therapist-1", 
            UserName = "therapist", 
            Email = "therapist@test.com" 
        };

        var availability = new TherapistAvailability
        {
            TherapistId = therapist.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsAvailable = true
        };

        await _context.Users.AddAsync(therapist);
        await _context.TherapistAvailability.AddAsync(availability);
        await _context.SaveChangesAsync();

        // Monday at 10:00 AM
        var requestTime = new DateTime(2025, 12, 15, 10, 0, 0); 

        // Act
        var result = await _service.IsTherapistAvailableAsync(
            therapist.Id,
            requestTime,
            requestTime.AddHours(1)
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task IsTherapistAvailableAsync_Should_Return_False_When_Outside_Working_Hours()
    {
        // Arrange
        var therapist = new ApplicationUser 
        { 
            Id = "therapist-1", 
            UserName = "therapist", 
            Email = "therapist@test.com" 
        };

        var availability = new TherapistAvailability
        {
            TherapistId = therapist.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsAvailable = true
        };

        await _context.Users.AddAsync(therapist);
        await _context.TherapistAvailability.AddAsync(availability);
        await _context.SaveChangesAsync();

        // Monday at 8:00 AM (before start time)
        var requestTime = new DateTime(2025, 12, 15, 8, 0, 0);

        // Act
        var result = await _service.IsTherapistAvailableAsync(
            therapist.Id,
            requestTime,
            requestTime.AddHours(1)
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsTherapistAvailableAsync_Should_Return_False_When_Has_Conflicting_Booking()
    {
        // Arrange
        var therapist = new ApplicationUser 
        { 
            Id = "therapist-1", 
            UserName = "therapist", 
            Email = "therapist@test.com" 
        };

        var availability = new TherapistAvailability
        {
            TherapistId = therapist.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsAvailable = true
        };

        // Existing booking from 10:00 to 11:00
        var existingBooking = new Booking
        {
            TherapistId = therapist.Id,
            ClientId = "client-1",
            ServiceId = 1,
            LocationId = 1,
            StartTime = new DateTime(2025, 12, 15, 10, 0, 0),
            EndTime = new DateTime(2025, 12, 15, 11, 0, 0),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Users.AddAsync(therapist);
        await _context.TherapistAvailability.AddAsync(availability);
        await _context.Bookings.AddAsync(existingBooking);
        await _context.SaveChangesAsync();

        // Try to book 10:30 to 11:30 (conflicts with existing booking)
        var requestTime = new DateTime(2025, 12, 15, 10, 30, 0);

        // Act
        var result = await _service.IsTherapistAvailableAsync(
            therapist.Id,
            requestTime,
            requestTime.AddHours(1)
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsTherapistAvailableAsync_Should_Consider_Buffer_Time()
    {
        // Arrange
        var therapist = new ApplicationUser 
        { 
            Id = "therapist-1", 
            UserName = "therapist", 
            Email = "therapist@test.com" 
        };

        var availability = new TherapistAvailability
        {
            TherapistId = therapist.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsAvailable = true
        };

        // Existing booking from 10:00 to 11:00
        var existingBooking = new Booking
        {
            TherapistId = therapist.Id,
            ClientId = "client-1",
            ServiceId = 1,
            LocationId = 1,
            StartTime = new DateTime(2025, 12, 15, 10, 0, 0),
            EndTime = new DateTime(2025, 12, 15, 11, 0, 0),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Users.AddAsync(therapist);
        await _context.TherapistAvailability.AddAsync(availability);
        await _context.Bookings.AddAsync(existingBooking);
        await _context.SaveChangesAsync();

        // Try to book 11:00 to 12:00 (should fail due to 15-minute buffer)
        var requestTime = new DateTime(2025, 12, 15, 11, 0, 0);

        // Act
        var result = await _service.IsTherapistAvailableAsync(
            therapist.Id,
            requestTime,
            requestTime.AddHours(1)
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse(); // Buffer time blocks this slot
    }

    [Fact]
    public async Task IsTherapistAvailableAsync_Should_Ignore_Cancelled_Bookings()
    {
        // Arrange
        var therapist = new ApplicationUser 
        { 
            Id = "therapist-1", 
            UserName = "therapist", 
            Email = "therapist@test.com" 
        };

        var availability = new TherapistAvailability
        {
            TherapistId = therapist.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsAvailable = true
        };

        // Cancelled booking from 10:00 to 11:00
        var cancelledBooking = new Booking
        {
            TherapistId = therapist.Id,
            ClientId = "client-1",
            ServiceId = 1,
            LocationId = 1,
            StartTime = new DateTime(2025, 12, 15, 10, 0, 0),
            EndTime = new DateTime(2025, 12, 15, 11, 0, 0),
            Status = BookingStatus.Cancelled,
            TotalPrice = 100m
        };

        await _context.Users.AddAsync(therapist);
        await _context.TherapistAvailability.AddAsync(availability);
        await _context.Bookings.AddAsync(cancelledBooking);
        await _context.SaveChangesAsync();

        // Try to book 10:00 to 11:00 (should succeed since previous was cancelled)
        var requestTime = new DateTime(2025, 12, 15, 10, 0, 0);

        // Act
        var result = await _service.IsTherapistAvailableAsync(
            therapist.Id,
            requestTime,
            requestTime.AddHours(1)
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task IsTherapistAvailableAsync_Should_Exclude_Specified_Booking()
    {
        // Arrange
        var therapist = new ApplicationUser 
        { 
            Id = "therapist-1", 
            UserName = "therapist", 
            Email = "therapist@test.com" 
        };

        var availability = new TherapistAvailability
        {
            TherapistId = therapist.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsAvailable = true
        };

        // Existing booking from 10:00 to 11:00
        var existingBooking = new Booking
        {
            Id = 1,
            TherapistId = therapist.Id,
            ClientId = "client-1",
            ServiceId = 1,
            LocationId = 1,
            StartTime = new DateTime(2025, 12, 15, 10, 0, 0),
            EndTime = new DateTime(2025, 12, 15, 11, 0, 0),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Users.AddAsync(therapist);
        await _context.TherapistAvailability.AddAsync(availability);
        await _context.Bookings.AddAsync(existingBooking);
        await _context.SaveChangesAsync();

        // Check availability for same time, excluding this booking (for update scenario)
        var requestTime = new DateTime(2025, 12, 15, 10, 0, 0);

        // Act
        var result = await _service.IsTherapistAvailableAsync(
            therapist.Id,
            requestTime,
            requestTime.AddHours(1),
            excludeBookingId: 1
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue(); // Should be available because we exclude booking 1
    }

    #endregion

    #region GetAvailableTherapistsAsync Tests

    [Fact]
    public async Task GetAvailableTherapistsAsync_Should_Return_Only_Available_Therapists()
    {
        // Arrange
        var therapist1 = new ApplicationUser { Id = "t1", UserName = "t1", Email = "t1@test.com" };
        var therapist2 = new ApplicationUser { Id = "t2", UserName = "t2", Email = "t2@test.com" };
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };

        var availability1 = new TherapistAvailability
        {
            TherapistId = therapist1.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsAvailable = true
        };

        var availability2 = new TherapistAvailability
        {
            TherapistId = therapist2.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsAvailable = true
        };

        var serviceTherapist1 = new ServiceTherapist { ServiceId = 1, TherapistId = therapist1.Id };
        var serviceTherapist2 = new ServiceTherapist { ServiceId = 1, TherapistId = therapist2.Id };

        // Therapist 2 has a booking
        var booking = new Booking
        {
            TherapistId = therapist2.Id,
            ClientId = "client-1",
            ServiceId = 1,
            LocationId = 1,
            StartTime = new DateTime(2025, 12, 15, 10, 0, 0),
            EndTime = new DateTime(2025, 12, 15, 11, 0, 0),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Users.AddRangeAsync(therapist1, therapist2);
        await _context.SpaServices.AddAsync(service);
        await _context.TherapistAvailability.AddRangeAsync(availability1, availability2);
        await _context.ServiceTherapists.AddRangeAsync(serviceTherapist1, serviceTherapist2);
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        var requestTime = new DateTime(2025, 12, 15, 10, 0, 0); // Monday 10 AM

        // Act
        var availableTherapists = await _service.GetAvailableTherapistsAsync(
            service.Id,
            requestTime,
            requestTime.AddHours(1)
        );

        // Assert
        availableTherapists.Should().HaveCount(1);
        availableTherapists[0].Id.Should().Be(therapist1.Id);
    }

    #endregion

    #region FindBestAvailableTherapistAsync Tests

    [Fact]
    public async Task FindBestAvailableTherapistAsync_Should_Return_Therapist_With_Least_Bookings()
    {
        // Arrange
        var therapist1 = new ApplicationUser { Id = "t1", UserName = "t1", Email = "t1@test.com" };
        var therapist2 = new ApplicationUser { Id = "t2", UserName = "t2", Email = "t2@test.com" };
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };

        var availability1 = new TherapistAvailability
        {
            TherapistId = therapist1.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsAvailable = true
        };

        var availability2 = new TherapistAvailability
        {
            TherapistId = therapist2.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsAvailable = true
        };

        var serviceTherapist1 = new ServiceTherapist { ServiceId = 1, TherapistId = therapist1.Id };
        var serviceTherapist2 = new ServiceTherapist { ServiceId = 1, TherapistId = therapist2.Id };

        // Therapist1 has 2 bookings, therapist2 has 0
        var booking1 = new Booking
        {
            TherapistId = therapist1.Id,
            ClientId = "client-1",
            ServiceId = 1,
            LocationId = 1,
            StartTime = new DateTime(2025, 12, 15, 9, 0, 0),
            EndTime = new DateTime(2025, 12, 15, 10, 0, 0),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        var booking2 = new Booking
        {
            TherapistId = therapist1.Id,
            ClientId = "client-2",
            ServiceId = 1,
            LocationId = 1,
            StartTime = new DateTime(2025, 12, 15, 11, 0, 0),
            EndTime = new DateTime(2025, 12, 15, 12, 0, 0),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Users.AddRangeAsync(therapist1, therapist2);
        await _context.SpaServices.AddAsync(service);
        await _context.TherapistAvailability.AddRangeAsync(availability1, availability2);
        await _context.ServiceTherapists.AddRangeAsync(serviceTherapist1, serviceTherapist2);
        await _context.Bookings.AddRangeAsync(booking1, booking2);
        await _context.SaveChangesAsync();

        var requestTime = new DateTime(2025, 12, 15, 14, 0, 0); // Monday 2 PM

        // Act
        var result = await _service.FindBestAvailableTherapistAsync(
            service.Id,
            requestTime,
            requestTime.AddHours(1)
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(therapist2.Id); // Therapist 2 has fewer bookings
    }

    [Fact]
    public async Task FindBestAvailableTherapistAsync_Should_Fail_When_No_Therapists_Available()
    {
        // Arrange
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };
        await _context.SpaServices.AddAsync(service);
        await _context.SaveChangesAsync();

        var requestTime = new DateTime(2025, 12, 15, 14, 0, 0);

        // Act
        var result = await _service.FindBestAvailableTherapistAsync(
            service.Id,
            requestTime,
            requestTime.AddHours(1)
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Therapist.NotAvailable");
    }

    #endregion

    #region GetQualifiedTherapistsForServiceAsync Tests

    [Fact]
    public async Task GetQualifiedTherapistsForServiceAsync_Should_Return_Only_Qualified_Therapists()
    {
        // Arrange
        var therapist1 = new ApplicationUser { Id = "t1", UserName = "t1", Email = "t1@test.com", FirstName = "Alice", LastName = "Smith" };
        var therapist2 = new ApplicationUser { Id = "t2", UserName = "t2", Email = "t2@test.com", FirstName = "Bob", LastName = "Jones" };
        var therapist3 = new ApplicationUser { Id = "t3", UserName = "t3", Email = "t3@test.com", FirstName = "Charlie", LastName = "Brown" };
        
        var service1 = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };
        var service2 = new SpaService { Id = 2, Name = "Facial", BasePrice = 80m, IsActive = true, LocationId = 1 };

        // Only therapist1 and therapist2 are qualified for service1
        var serviceTherapist1 = new ServiceTherapist { ServiceId = 1, TherapistId = therapist1.Id };
        var serviceTherapist2 = new ServiceTherapist { ServiceId = 1, TherapistId = therapist2.Id };
        var serviceTherapist3 = new ServiceTherapist { ServiceId = 2, TherapistId = therapist3.Id }; // Different service

        await _context.Users.AddRangeAsync(therapist1, therapist2, therapist3);
        await _context.SpaServices.AddRangeAsync(service1, service2);
        await _context.ServiceTherapists.AddRangeAsync(serviceTherapist1, serviceTherapist2, serviceTherapist3);
        await _context.SaveChangesAsync();

        // Act
        var qualifiedTherapists = await _service.GetQualifiedTherapistsForServiceAsync(1);

        // Assert
        qualifiedTherapists.Should().HaveCount(2);
        qualifiedTherapists.Should().Contain(t => t.Id == therapist1.Id);
        qualifiedTherapists.Should().Contain(t => t.Id == therapist2.Id);
        qualifiedTherapists.Should().NotContain(t => t.Id == therapist3.Id);
    }

    [Fact]
    public async Task GetQualifiedTherapistsForServiceAsync_Should_Return_Therapists_Sorted_By_Name()
    {
        // Arrange
        var therapist1 = new ApplicationUser { Id = "t1", UserName = "t1", Email = "t1@test.com", FirstName = "Charlie", LastName = "Brown" };
        var therapist2 = new ApplicationUser { Id = "t2", UserName = "t2", Email = "t2@test.com", FirstName = "Alice", LastName = "Smith" };
        var therapist3 = new ApplicationUser { Id = "t3", UserName = "t3", Email = "t3@test.com", FirstName = "Bob", LastName = "Jones" };
        
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };

        var serviceTherapist1 = new ServiceTherapist { ServiceId = 1, TherapistId = therapist1.Id };
        var serviceTherapist2 = new ServiceTherapist { ServiceId = 1, TherapistId = therapist2.Id };
        var serviceTherapist3 = new ServiceTherapist { ServiceId = 1, TherapistId = therapist3.Id };

        await _context.Users.AddRangeAsync(therapist1, therapist2, therapist3);
        await _context.SpaServices.AddAsync(service);
        await _context.ServiceTherapists.AddRangeAsync(serviceTherapist1, serviceTherapist2, serviceTherapist3);
        await _context.SaveChangesAsync();

        // Act
        var qualifiedTherapists = await _service.GetQualifiedTherapistsForServiceAsync(1);

        // Assert
        qualifiedTherapists.Should().HaveCount(3);
        qualifiedTherapists[0].FirstName.Should().Be("Alice"); // Alphabetically first
        qualifiedTherapists[1].FirstName.Should().Be("Bob");
        qualifiedTherapists[2].FirstName.Should().Be("Charlie");
    }

    #endregion

    #region GetTherapistScheduleAsync Tests

    [Fact]
    public async Task GetTherapistScheduleAsync_Should_Return_All_Availability_Records()
    {
        // Arrange
        var therapist = new ApplicationUser { Id = "t1", UserName = "t1", Email = "t1@test.com" };

        var availability1 = new TherapistAvailability
        {
            TherapistId = therapist.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsAvailable = true
        };

        var availability2 = new TherapistAvailability
        {
            TherapistId = therapist.Id,
            DayOfWeek = DayOfWeek.Tuesday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsAvailable = true
        };

        var availability3 = new TherapistAvailability
        {
            TherapistId = therapist.Id,
            DayOfWeek = DayOfWeek.Wednesday,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(16, 0, 0),
            IsAvailable = true
        };

        await _context.Users.AddAsync(therapist);
        await _context.TherapistAvailability.AddRangeAsync(availability1, availability2, availability3);
        await _context.SaveChangesAsync();

        // Act
        var schedule = await _service.GetTherapistScheduleAsync(
            therapist.Id,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7)
        );

        // Assert
        schedule.Should().HaveCount(3);
        schedule[0].DayOfWeek.Should().Be(DayOfWeek.Monday);
        schedule[1].DayOfWeek.Should().Be(DayOfWeek.Tuesday);
        schedule[2].DayOfWeek.Should().Be(DayOfWeek.Wednesday);
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

