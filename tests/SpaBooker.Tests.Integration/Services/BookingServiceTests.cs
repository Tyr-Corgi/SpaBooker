using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SpaBooker.Core.DTOs.Booking;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;
using SpaBooker.Core.Interfaces;
using SpaBooker.Infrastructure.Data;
using SpaBooker.Infrastructure.Services;

namespace SpaBooker.Tests.Integration.Services;

public class BookingServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly BookingService _service;
    private readonly Mock<IBookingConflictChecker> _conflictCheckerMock;
    private readonly Mock<IRoomAvailabilityService> _roomAvailabilityMock;
    private readonly Mock<ITherapistAvailabilityService> _therapistAvailabilityMock;
    private readonly Mock<IAuditService> _auditServiceMock;

    public BookingServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _conflictCheckerMock = new Mock<IBookingConflictChecker>();
        _roomAvailabilityMock = new Mock<IRoomAvailabilityService>();
        _therapistAvailabilityMock = new Mock<ITherapistAvailabilityService>();
        _auditServiceMock = new Mock<IAuditService>();

        _service = new BookingService(
            _context,
            _conflictCheckerMock.Object,
            _roomAvailabilityMock.Object,
            _therapistAvailabilityMock.Object,
            _auditServiceMock.Object
        );
    }

    #region CreateBookingAsync Tests

    [Fact]
    public async Task CreateBookingAsync_Should_Create_Booking_When_Valid()
    {
        // Arrange
        var client = new ApplicationUser { Id = "client-1", UserName = "client", Email = "client@test.com" };
        var therapist = new ApplicationUser { Id = "therapist-1", UserName = "therapist", Email = "therapist@test.com" };
        var location = new Location { Id = 1, Name = "Test Spa", City = "Test City", State = "TS", ZipCode = "12345" };
        var service = new SpaService 
        { 
            Id = 1, 
            Name = "Massage", 
            BasePrice = 100.00m, 
            IsActive = true,
            LocationId = 1
        };

        await _context.Users.AddRangeAsync(client, therapist);
        await _context.Locations.AddAsync(location);
        await _context.SpaServices.AddAsync(service);
        await _context.SaveChangesAsync();

        var dto = new CreateBookingDto
        {
            ClientId = client.Id,
            ServiceId = service.Id,
            TherapistId = therapist.Id,
            LocationId = location.Id,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Notes = "Test booking"
        };

        _therapistAvailabilityMock
            .Setup(x => x.IsTherapistAvailableAsync(therapist.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
            .ReturnsAsync(Core.Common.Result.Success(true));

        // Act
        var result = await _service.CreateBookingAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ClientId.Should().Be(client.Id);
        result.Value.TherapistId.Should().Be(therapist.Id);
        result.Value.Status.Should().Be(BookingStatus.Confirmed);
        result.Value.TotalPrice.Should().Be(100.00m);

        var savedBooking = await _context.Bookings.FirstOrDefaultAsync();
        savedBooking.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateBookingAsync_Should_Fail_When_Service_Not_Found()
    {
        // Arrange
        var client = new ApplicationUser { Id = "client-1", UserName = "client", Email = "client@test.com" };
        await _context.Users.AddAsync(client);
        await _context.SaveChangesAsync();

        var dto = new CreateBookingDto
        {
            ClientId = client.Id,
            ServiceId = 999, // Non-existent
            TherapistId = "therapist-1",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
        };

        // Act
        var result = await _service.CreateBookingAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Service.NotFound");
    }

    [Fact]
    public async Task CreateBookingAsync_Should_Fail_When_Service_Inactive()
    {
        // Arrange
        var client = new ApplicationUser { Id = "client-1", UserName = "client", Email = "client@test.com" };
        var service = new SpaService 
        { 
            Id = 1, 
            Name = "Massage", 
            BasePrice = 100.00m, 
            IsActive = false, // Inactive
            LocationId = 1
        };

        await _context.Users.AddAsync(client);
        await _context.SpaServices.AddAsync(service);
        await _context.SaveChangesAsync();

        var dto = new CreateBookingDto
        {
            ClientId = client.Id,
            ServiceId = service.Id,
            TherapistId = "therapist-1",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
        };

        // Act
        var result = await _service.CreateBookingAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Service.NotActive");
    }

    [Fact]
    public async Task CreateBookingAsync_Should_Fail_When_Therapist_Not_Available()
    {
        // Arrange
        var client = new ApplicationUser { Id = "client-1", UserName = "client", Email = "client@test.com" };
        var therapist = new ApplicationUser { Id = "therapist-1", UserName = "therapist", Email = "therapist@test.com" };
        var service = new SpaService 
        { 
            Id = 1, 
            Name = "Massage", 
            BasePrice = 100.00m, 
            IsActive = true,
            LocationId = 1
        };

        await _context.Users.AddRangeAsync(client, therapist);
        await _context.SpaServices.AddAsync(service);
        await _context.SaveChangesAsync();

        var dto = new CreateBookingDto
        {
            ClientId = client.Id,
            ServiceId = service.Id,
            TherapistId = therapist.Id,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
        };

        _therapistAvailabilityMock
            .Setup(x => x.IsTherapistAvailableAsync(therapist.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
            .ReturnsAsync(Core.Common.Result.Success(false));

        // Act
        var result = await _service.CreateBookingAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Therapist.NotAvailable");
    }

    #endregion

    #region UpdateBookingAsync Tests

    [Fact]
    public async Task UpdateBookingAsync_Should_Update_Booking_When_Valid()
    {
        // Arrange
        var client = new ApplicationUser { Id = "client-1", UserName = "client", Email = "client@test.com" };
        var therapist = new ApplicationUser { Id = "therapist-1", UserName = "therapist", Email = "therapist@test.com" };
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };
        
        var booking = new Booking
        {
            Id = 1,
            ClientId = client.Id,
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m,
            Notes = "Original notes"
        };

        await _context.Users.AddRangeAsync(client, therapist);
        await _context.SpaServices.AddAsync(service);
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateBookingDto
        {
            Id = booking.Id,
            Notes = "Updated notes"
        };

        // Act
        var result = await _service.UpdateBookingAsync(updateDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Notes.Should().Be("Updated notes");
        
        var updatedBooking = await _context.Bookings.FindAsync(booking.Id);
        updatedBooking!.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task UpdateBookingAsync_Should_Fail_When_Booking_Not_Found()
    {
        // Arrange
        var updateDto = new UpdateBookingDto
        {
            Id = 999, // Non-existent
            Notes = "Updated notes"
        };

        // Act
        var result = await _service.UpdateBookingAsync(updateDto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Booking.NotFound");
    }

    [Fact]
    public async Task UpdateBookingAsync_Should_Check_Availability_When_Changing_Time()
    {
        // Arrange
        var client = new ApplicationUser { Id = "client-1", UserName = "client", Email = "client@test.com" };
        var therapist = new ApplicationUser { Id = "therapist-1", UserName = "therapist", Email = "therapist@test.com" };
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };
        
        var booking = new Booking
        {
            Id = 1,
            ClientId = client.Id,
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Users.AddRangeAsync(client, therapist);
        await _context.SpaServices.AddAsync(service);
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        var newStartTime = DateTime.UtcNow.AddDays(2);
        var newEndTime = DateTime.UtcNow.AddDays(2).AddHours(1);

        var updateDto = new UpdateBookingDto
        {
            Id = booking.Id,
            StartTime = newStartTime,
            EndTime = newEndTime
        };

        _therapistAvailabilityMock
            .Setup(x => x.IsTherapistAvailableAsync(therapist.Id, newStartTime, newEndTime, booking.Id))
            .ReturnsAsync(Core.Common.Result.Success(true));

        // Act
        var result = await _service.UpdateBookingAsync(updateDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.StartTime.Should().Be(newStartTime);
        result.Value.EndTime.Should().Be(newEndTime);
    }

    #endregion

    #region CancelBookingAsync Tests

    [Fact]
    public async Task CancelBookingAsync_Should_Cancel_Booking()
    {
        // Arrange
        var booking = new Booking
        {
            Id = 1,
            ClientId = "client-1",
            TherapistId = "therapist-1",
            ServiceId = 1,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CancelBookingAsync(booking.Id, "Client requested cancellation");

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var cancelledBooking = await _context.Bookings.FindAsync(booking.Id);
        cancelledBooking!.Status.Should().Be(BookingStatus.Cancelled);
        cancelledBooking.Notes.Should().Contain("Client requested cancellation");
    }

    [Fact]
    public async Task CancelBookingAsync_Should_Fail_When_Booking_Not_Found()
    {
        // Act
        var result = await _service.CancelBookingAsync(999, "Reason");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Booking.NotFound");
    }

    #endregion

    #region Status Change Tests

    [Fact]
    public async Task ConfirmBookingAsync_Should_Set_Status_To_Confirmed()
    {
        // Arrange
        var booking = new Booking
        {
            Id = 1,
            ClientId = "client-1",
            TherapistId = "therapist-1",
            ServiceId = 1,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Status = BookingStatus.Pending,
            TotalPrice = 100m
        };

        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ConfirmBookingAsync(booking.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var confirmedBooking = await _context.Bookings.FindAsync(booking.Id);
        confirmedBooking!.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task CompleteBookingAsync_Should_Set_Status_To_Completed()
    {
        // Arrange
        var booking = new Booking
        {
            Id = 1,
            ClientId = "client-1",
            TherapistId = "therapist-1",
            ServiceId = 1,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow.AddDays(-1).AddHours(1),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CompleteBookingAsync(booking.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var completedBooking = await _context.Bookings.FindAsync(booking.Id);
        completedBooking!.Status.Should().Be(BookingStatus.Completed);
    }

    [Fact]
    public async Task MarkAsNoShowAsync_Should_Set_Status_To_NoShow()
    {
        // Arrange
        var booking = new Booking
        {
            Id = 1,
            ClientId = "client-1",
            TherapistId = "therapist-1",
            ServiceId = 1,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow.AddDays(-1).AddHours(1),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.MarkAsNoShowAsync(booking.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var noShowBooking = await _context.Bookings.FindAsync(booking.Id);
        noShowBooking!.Status.Should().Be(BookingStatus.NoShow);
    }

    #endregion

    #region Query Tests

    [Fact]
    public async Task GetBookingsForDateAsync_Should_Return_Bookings_For_Specific_Date()
    {
        // Arrange
        var targetDate = new DateTime(2025, 12, 15);
        var client = new ApplicationUser { Id = "client-1", UserName = "client", Email = "client@test.com" };
        var therapist = new ApplicationUser { Id = "therapist-1", UserName = "therapist", Email = "therapist@test.com", FirstName = "John", LastName = "Doe" };
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };
        var location = new Location { Id = 1, Name = "Spa", City = "City", State = "ST", ZipCode = "12345" };

        var booking1 = new Booking
        {
            Id = 1,
            ClientId = client.Id,
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = targetDate.AddHours(10),
            EndTime = targetDate.AddHours(11),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        var booking2 = new Booking
        {
            Id = 2,
            ClientId = client.Id,
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = targetDate.AddDays(1).AddHours(10), // Different day
            EndTime = targetDate.AddDays(1).AddHours(11),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Users.AddRangeAsync(client, therapist);
        await _context.SpaServices.AddAsync(service);
        await _context.Locations.AddAsync(location);
        await _context.Bookings.AddRangeAsync(booking1, booking2);
        await _context.SaveChangesAsync();

        // Act
        var bookings = await _service.GetBookingsForDateAsync(targetDate);

        // Assert
        bookings.Should().HaveCount(1);
        bookings[0].Id.Should().Be(booking1.Id);
    }

    [Fact]
    public async Task GetBookingsForTherapistAsync_Should_Return_Therapist_Bookings()
    {
        // Arrange
        var targetDate = new DateTime(2025, 12, 15);
        var client = new ApplicationUser { Id = "client-1", UserName = "client", Email = "client@test.com", FirstName = "Jane", LastName = "Smith" };
        var therapist1 = new ApplicationUser { Id = "therapist-1", UserName = "therapist1", Email = "t1@test.com", FirstName = "John", LastName = "Doe" };
        var therapist2 = new ApplicationUser { Id = "therapist-2", UserName = "therapist2", Email = "t2@test.com", FirstName = "Mary", LastName = "Jane" };
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };

        var booking1 = new Booking
        {
            ClientId = client.Id,
            TherapistId = therapist1.Id,
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = targetDate.AddHours(10),
            EndTime = targetDate.AddHours(11),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        var booking2 = new Booking
        {
            ClientId = client.Id,
            TherapistId = therapist2.Id, // Different therapist
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = targetDate.AddHours(14),
            EndTime = targetDate.AddHours(15),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Users.AddRangeAsync(client, therapist1, therapist2);
        await _context.SpaServices.AddAsync(service);
        await _context.Bookings.AddRangeAsync(booking1, booking2);
        await _context.SaveChangesAsync();

        // Act
        var bookings = await _service.GetBookingsForTherapistAsync(therapist1.Id, targetDate);

        // Assert
        bookings.Should().HaveCount(1);
        bookings[0].TherapistId.Should().Be(therapist1.Id);
    }

    [Fact]
    public async Task GetClientBookingsAsync_Should_Return_Client_Bookings()
    {
        // Arrange
        var client1 = new ApplicationUser { Id = "client-1", UserName = "client1", Email = "c1@test.com", FirstName = "Jane", LastName = "Smith" };
        var client2 = new ApplicationUser { Id = "client-2", UserName = "client2", Email = "c2@test.com", FirstName = "Bob", LastName = "Johnson" };
        var therapist = new ApplicationUser { Id = "therapist-1", UserName = "therapist", Email = "t@test.com", FirstName = "John", LastName = "Doe" };
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };

        var booking1 = new Booking
        {
            ClientId = client1.Id,
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        var booking2 = new Booking
        {
            ClientId = client2.Id, // Different client
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(2).AddHours(1),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Users.AddRangeAsync(client1, client2, therapist);
        await _context.SpaServices.AddAsync(service);
        await _context.Bookings.AddRangeAsync(booking1, booking2);
        await _context.SaveChangesAsync();

        // Act
        var bookings = await _service.GetClientBookingsAsync(client1.Id);

        // Assert
        bookings.Should().HaveCount(1);
        bookings[0].ClientId.Should().Be(client1.Id);
    }

    [Fact]
    public async Task GetBookingByIdAsync_Should_Return_Booking_When_Found()
    {
        // Arrange
        var client = new ApplicationUser { Id = "client-1", UserName = "client", Email = "c@test.com", FirstName = "Jane", LastName = "Smith" };
        var therapist = new ApplicationUser { Id = "therapist-1", UserName = "therapist", Email = "t@test.com", FirstName = "John", LastName = "Doe" };
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };

        var booking = new Booking
        {
            Id = 1,
            ClientId = client.Id,
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Users.AddRangeAsync(client, therapist);
        await _context.SpaServices.AddAsync(service);
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetBookingByIdAsync(booking.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(booking.Id);
    }

    [Fact]
    public async Task GetBookingByIdAsync_Should_Fail_When_Not_Found()
    {
        // Act
        var result = await _service.GetBookingByIdAsync(999);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Booking.NotFound");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ValidateBookingAsync_Should_Fail_When_EndTime_Before_StartTime()
    {
        // Arrange
        var dto = new CreateBookingDto
        {
            ClientId = "client-1",
            ServiceId = 1,
            TherapistId = "therapist-1",
            StartTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1) // Before start time
        };

        // Act
        var result = await _service.ValidateBookingAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Booking.InvalidTimeSlot");
    }

    [Fact]
    public async Task ValidateBookingAsync_Should_Fail_When_Client_Not_Found()
    {
        // Arrange
        var dto = new CreateBookingDto
        {
            ClientId = "non-existent-client",
            ServiceId = 1,
            TherapistId = "therapist-1",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
        };

        // Act
        var result = await _service.ValidateBookingAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Client.NotFound");
    }

    #endregion

    #region RescheduleBookingAsync Tests

    [Fact]
    public async Task RescheduleBookingAsync_Should_Reschedule_Successfully()
    {
        // Arrange
        var client = new ApplicationUser { Id = "client-1", UserName = "client", Email = "client@test.com", FirstName = "John", LastName = "Doe" };
        var therapist = new ApplicationUser { Id = "therapist-1", UserName = "therapist", Email = "therapist@test.com", FirstName = "Jane", LastName = "Smith" };
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };

        var booking = new Booking
        {
            Id = 1,
            ClientId = client.Id,
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(5), // 5 days in future (> 24 hours)
            EndTime = DateTime.UtcNow.AddDays(5).AddHours(1),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Users.AddRangeAsync(client, therapist);
        await _context.SpaServices.AddAsync(service);
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        var rescheduleDto = new SpaBooker.Core.DTOs.Booking.RescheduleBookingDto
        {
            BookingId = booking.Id,
            NewStartTime = DateTime.UtcNow.AddDays(7), // Reschedule to 7 days from now
            NewEndTime = DateTime.UtcNow.AddDays(7).AddHours(1),
            RescheduleReason = "Schedule conflict"
        };

        _therapistAvailabilityMock
            .Setup(x => x.IsTherapistAvailableAsync(therapist.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(), booking.Id))
            .ReturnsAsync(Core.Common.Result.Success(true));

        // Act
        var result = await _service.RescheduleBookingAsync(rescheduleDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.StartTime.Should().BeCloseTo(rescheduleDto.NewStartTime, TimeSpan.FromSeconds(1));
        result.Value.EndTime.Should().BeCloseTo(rescheduleDto.NewEndTime, TimeSpan.FromSeconds(1));
        result.Value.Notes.Should().Contain("Rescheduled");
        result.Value.Notes.Should().Contain("Schedule conflict");
    }

    [Fact]
    public async Task RescheduleBookingAsync_Should_Fail_When_Booking_Not_Found()
    {
        // Arrange
        var rescheduleDto = new SpaBooker.Core.DTOs.Booking.RescheduleBookingDto
        {
            BookingId = 999,
            NewStartTime = DateTime.UtcNow.AddDays(7),
            NewEndTime = DateTime.UtcNow.AddDays(7).AddHours(1)
        };

        // Act
        var result = await _service.RescheduleBookingAsync(rescheduleDto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Booking.NotFound");
    }

    [Fact]
    public async Task RescheduleBookingAsync_Should_Fail_When_Less_Than_24_Hours()
    {
        // Arrange
        var client = new ApplicationUser { Id = "client-1", UserName = "client", Email = "client@test.com" };
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };

        var booking = new Booking
        {
            Id = 1,
            ClientId = client.Id,
            TherapistId = "therapist-1",
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddHours(12), // Only 12 hours away
            EndTime = DateTime.UtcNow.AddHours(13),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Users.AddAsync(client);
        await _context.SpaServices.AddAsync(service);
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        var rescheduleDto = new SpaBooker.Core.DTOs.Booking.RescheduleBookingDto
        {
            BookingId = booking.Id,
            NewStartTime = DateTime.UtcNow.AddDays(7),
            NewEndTime = DateTime.UtcNow.AddDays(7).AddHours(1)
        };

        // Act
        var result = await _service.RescheduleBookingAsync(rescheduleDto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Booking.RescheduleTooLate");
    }

    [Fact]
    public async Task RescheduleBookingAsync_Should_Fail_When_Therapist_Not_Available()
    {
        // Arrange
        var client = new ApplicationUser { Id = "client-1", UserName = "client", Email = "client@test.com" };
        var therapist = new ApplicationUser { Id = "therapist-1", UserName = "therapist", Email = "therapist@test.com" };
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };

        var booking = new Booking
        {
            Id = 1,
            ClientId = client.Id,
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(5),
            EndTime = DateTime.UtcNow.AddDays(5).AddHours(1),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Users.AddRangeAsync(client, therapist);
        await _context.SpaServices.AddAsync(service);
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        var rescheduleDto = new SpaBooker.Core.DTOs.Booking.RescheduleBookingDto
        {
            BookingId = booking.Id,
            NewStartTime = DateTime.UtcNow.AddDays(7),
            NewEndTime = DateTime.UtcNow.AddDays(7).AddHours(1)
        };

        _therapistAvailabilityMock
            .Setup(x => x.IsTherapistAvailableAsync(therapist.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(), booking.Id))
            .ReturnsAsync(Core.Common.Result.Success(false));

        // Act
        var result = await _service.RescheduleBookingAsync(rescheduleDto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Therapist.NotAvailable");
    }

    [Fact]
    public async Task RescheduleBookingAsync_Should_Fail_When_Invalid_Time_Slot()
    {
        // Arrange
        var client = new ApplicationUser { Id = "client-1", UserName = "client", Email = "client@test.com" };
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };

        var booking = new Booking
        {
            Id = 1,
            ClientId = client.Id,
            TherapistId = "therapist-1",
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(5),
            EndTime = DateTime.UtcNow.AddDays(5).AddHours(1),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Users.AddAsync(client);
        await _context.SpaServices.AddAsync(service);
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        var rescheduleDto = new SpaBooker.Core.DTOs.Booking.RescheduleBookingDto
        {
            BookingId = booking.Id,
            NewStartTime = DateTime.UtcNow.AddDays(7).AddHours(2),
            NewEndTime = DateTime.UtcNow.AddDays(7).AddHours(1) // End before start!
        };

        // Act
        var result = await _service.RescheduleBookingAsync(rescheduleDto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Booking.InvalidTimeSlot");
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

