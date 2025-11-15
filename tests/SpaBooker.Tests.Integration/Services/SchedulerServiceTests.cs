using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;
using SpaBooker.Infrastructure.Data;

namespace SpaBooker.Tests.Integration.Services;

public class SchedulerServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public SchedulerServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Should_Create_Booking_With_Room_And_Therapist_Assignment()
    {
        // Arrange
        var location = new Location 
        { 
            Name = "Test Spa", 
            Address = "123 Test St", 
            City = "Test City", 
            State = "TS", 
            ZipCode = "12345",
            IsActive = true 
        };
        var room = new Room 
        { 
            Name = "Room 1", 
            Location = location, 
            IsActive = true,
            ColorCode = "#007bff"
        };
        var client = new ApplicationUser 
        { 
            Id = "client-1", 
            UserName = "client@test.com", 
            Email = "client@test.com",
            FirstName = "John",
            LastName = "Doe"
        };
        var therapist = new ApplicationUser 
        { 
            Id = "therapist-1", 
            UserName = "therapist@test.com", 
            Email = "therapist@test.com",
            FirstName = "Jane",
            LastName = "Smith"
        };
        var service = new SpaService 
        { 
            Name = "Massage", 
            DurationMinutes = 60, 
            BasePrice = 100, 
            Location = location,
            IsActive = true
        };

        await _context.Locations.AddAsync(location);
        await _context.Rooms.AddAsync(room);
        await _context.Users.AddRangeAsync(client, therapist);
        await _context.SpaServices.AddAsync(service);
        await _context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.Date.AddHours(10);
        var endTime = startTime.AddMinutes(service.DurationMinutes);

        // Act
        var booking = new Booking
        {
            ClientId = client.Id,
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            LocationId = location.Id,
            RoomId = room.Id,
            StartTime = startTime,
            EndTime = endTime,
            Status = BookingStatus.Confirmed,
            ServicePrice = service.BasePrice,
            TotalPrice = service.BasePrice,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Assert
        var savedBooking = await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.Therapist)
            .Include(b => b.Client)
            .FirstAsync(b => b.Id == booking.Id);

        savedBooking.Should().NotBeNull();
        savedBooking.RoomId.Should().Be(room.Id);
        savedBooking.TherapistId.Should().Be(therapist.Id);
        savedBooking.ClientId.Should().Be(client.Id);
        savedBooking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task Should_Detect_Therapist_Conflict()
    {
        // Arrange
        var location = new Location 
        { 
            Name = "Test Spa", 
            Address = "123 Test St", 
            City = "Test City", 
            State = "TS", 
            ZipCode = "12345",
            IsActive = true 
        };
        var therapist = new ApplicationUser 
        { 
            Id = "therapist-1", 
            UserName = "therapist@test.com", 
            Email = "therapist@test.com",
            FirstName = "Jane",
            LastName = "Smith"
        };
        var service = new SpaService 
        { 
            Name = "Massage", 
            DurationMinutes = 60, 
            BasePrice = 100, 
            Location = location,
            IsActive = true
        };

        await _context.Locations.AddAsync(location);
        await _context.Users.AddAsync(therapist);
        await _context.SpaServices.AddAsync(service);
        await _context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.Date.AddHours(10);
        var endTime = startTime.AddMinutes(60);

        var existingBooking = new Booking
        {
            ClientId = "client-1",
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            LocationId = location.Id,
            StartTime = startTime,
            EndTime = endTime,
            Status = BookingStatus.Confirmed,
            ServicePrice = service.BasePrice,
            TotalPrice = service.BasePrice,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(existingBooking);
        await _context.SaveChangesAsync();

        // Act - Try to create overlapping booking
        var overlappingStart = startTime.AddMinutes(30);
        var overlappingEnd = overlappingStart.AddMinutes(60);

        var bookings = await _context.Bookings
            .Where(b => b.TherapistId == therapist.Id 
                     && b.Status != BookingStatus.Cancelled)
            .ToListAsync();

        var hasConflict = bookings.Any(b =>
            (overlappingStart >= b.StartTime && overlappingStart < b.EndTime) ||
            (overlappingEnd > b.StartTime && overlappingEnd <= b.EndTime) ||
            (overlappingStart <= b.StartTime && overlappingEnd >= b.EndTime));

        // Assert
        hasConflict.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Detect_Room_Conflict()
    {
        // Arrange
        var location = new Location 
        { 
            Name = "Test Spa", 
            Address = "123 Test St", 
            City = "Test City", 
            State = "TS", 
            ZipCode = "12345",
            IsActive = true 
        };
        var room = new Room 
        { 
            Name = "Room 1", 
            Location = location, 
            IsActive = true,
            ColorCode = "#007bff"
        };
        var service = new SpaService 
        { 
            Name = "Massage", 
            DurationMinutes = 60, 
            BasePrice = 100, 
            Location = location,
            IsActive = true
        };

        await _context.Locations.AddAsync(location);
        await _context.Rooms.AddAsync(room);
        await _context.SpaServices.AddAsync(service);
        await _context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.Date.AddHours(14);
        var endTime = startTime.AddMinutes(60);

        var existingBooking = new Booking
        {
            ClientId = "client-1",
            TherapistId = "therapist-1",
            ServiceId = service.Id,
            LocationId = location.Id,
            RoomId = room.Id,
            StartTime = startTime,
            EndTime = endTime,
            Status = BookingStatus.Confirmed,
            ServicePrice = service.BasePrice,
            TotalPrice = service.BasePrice,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(existingBooking);
        await _context.SaveChangesAsync();

        // Act - Check for room availability
        var checkStart = startTime.AddMinutes(15);
        var checkEnd = checkStart.AddMinutes(60);

        var bookings = await _context.Bookings
            .Where(b => b.RoomId == room.Id 
                     && b.Status != BookingStatus.Cancelled)
            .ToListAsync();

        var hasConflict = bookings.Any(b =>
            (checkStart >= b.StartTime && checkStart < b.EndTime) ||
            (checkEnd > b.StartTime && checkEnd <= b.EndTime) ||
            (checkStart <= b.StartTime && checkEnd >= b.EndTime));

        // Assert
        hasConflict.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Allow_Booking_Reassignment()
    {
        // Arrange
        var location = new Location 
        { 
            Name = "Test Spa", 
            Address = "123 Test St", 
            City = "Test City", 
            State = "TS", 
            ZipCode = "12345",
            IsActive = true 
        };
        var room1 = new Room 
        { 
            Name = "Room 1", 
            Location = location, 
            IsActive = true,
            ColorCode = "#007bff"
        };
        var room2 = new Room 
        { 
            Name = "Room 2", 
            Location = location, 
            IsActive = true,
            ColorCode = "#28a745"
        };
        var therapist1 = new ApplicationUser 
        { 
            Id = "therapist-1", 
            UserName = "therapist1@test.com", 
            Email = "therapist1@test.com",
            FirstName = "Jane",
            LastName = "Smith"
        };
        var therapist2 = new ApplicationUser 
        { 
            Id = "therapist-2", 
            UserName = "therapist2@test.com", 
            Email = "therapist2@test.com",
            FirstName = "John",
            LastName = "Doe"
        };
        var service = new SpaService 
        { 
            Name = "Massage", 
            DurationMinutes = 60, 
            BasePrice = 100, 
            Location = location,
            IsActive = true
        };

        await _context.Locations.AddAsync(location);
        await _context.Rooms.AddRangeAsync(room1, room2);
        await _context.Users.AddRangeAsync(therapist1, therapist2);
        await _context.SpaServices.AddAsync(service);
        await _context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.Date.AddHours(16);
        var endTime = startTime.AddMinutes(60);

        var booking = new Booking
        {
            ClientId = "client-1",
            TherapistId = therapist1.Id,
            ServiceId = service.Id,
            LocationId = location.Id,
            RoomId = room1.Id,
            StartTime = startTime,
            EndTime = endTime,
            Status = BookingStatus.Confirmed,
            ServicePrice = service.BasePrice,
            TotalPrice = service.BasePrice,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Act - Reassign room and therapist
        booking.RoomId = room2.Id;
        booking.TherapistId = therapist2.Id;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Assert
        var updatedBooking = await _context.Bookings.FindAsync(booking.Id);
        updatedBooking!.RoomId.Should().Be(room2.Id);
        updatedBooking.TherapistId.Should().Be(therapist2.Id);
        updatedBooking.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_Filter_Bookings_By_Location_And_Date()
    {
        // Arrange
        var location1 = new Location 
        { 
            Name = "Spa 1", 
            Address = "123 Test St", 
            City = "Test City", 
            State = "TS", 
            ZipCode = "12345",
            IsActive = true 
        };
        var location2 = new Location 
        { 
            Name = "Spa 2", 
            Address = "456 Test Ave", 
            City = "Test City", 
            State = "TS", 
            ZipCode = "12345",
            IsActive = true 
        };
        var service = new SpaService 
        { 
            Name = "Massage", 
            DurationMinutes = 60, 
            BasePrice = 100, 
            Location = location1,
            IsActive = true 
        };

        await _context.Locations.AddRangeAsync(location1, location2);
        await _context.SpaServices.AddAsync(service);
        await _context.SaveChangesAsync();

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var booking1 = new Booking
        {
            ClientId = "client-1",
            TherapistId = "therapist-1",
            ServiceId = service.Id,
            LocationId = location1.Id,
            StartTime = today.AddHours(10),
            EndTime = today.AddHours(11),
            Status = BookingStatus.Confirmed,
            ServicePrice = 100,
            TotalPrice = 100,
            CreatedAt = DateTime.UtcNow
        };

        var booking2 = new Booking
        {
            ClientId = "client-2",
            TherapistId = "therapist-2",
            ServiceId = service.Id,
            LocationId = location2.Id,
            StartTime = today.AddHours(14),
            EndTime = today.AddHours(15),
            Status = BookingStatus.Confirmed,
            ServicePrice = 100,
            TotalPrice = 100,
            CreatedAt = DateTime.UtcNow
        };

        var booking3 = new Booking
        {
            ClientId = "client-3",
            TherapistId = "therapist-1",
            ServiceId = service.Id,
            LocationId = location1.Id,
            StartTime = tomorrow.AddHours(10),
            EndTime = tomorrow.AddHours(11),
            Status = BookingStatus.Confirmed,
            ServicePrice = 100,
            TotalPrice = 100,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.AddRange(booking1, booking2, booking3);
        await _context.SaveChangesAsync();

        // Act
        var filteredBookings = await _context.Bookings
            .Where(b => b.LocationId == location1.Id 
                     && b.StartTime >= today 
                     && b.StartTime < today.AddDays(1)
                     && b.Status != BookingStatus.Cancelled)
            .ToListAsync();

        // Assert
        filteredBookings.Should().HaveCount(1);
        filteredBookings.First().Id.Should().Be(booking1.Id);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

