using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;
using SpaBooker.Core.Settings;
using SpaBooker.Infrastructure.Data;
using SpaBooker.Infrastructure.Services;

namespace SpaBooker.Tests.Integration.Services;

public class RoomAvailabilityServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly RoomAvailabilityService _service;

    public RoomAvailabilityServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var bufferSettings = new BufferTimeSettings { BufferMinutes = 15 };
        var optionsWrapper = Options.Create(bufferSettings);

        _service = new RoomAvailabilityService(_context, optionsWrapper);
    }

    #region IsRoomAvailableAsync Tests

    [Fact]
    public async Task IsRoomAvailableAsync_Should_Return_False_When_Room_Not_Found()
    {
        // Act
        var result = await _service.IsRoomAvailableAsync(
            999,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1)
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Room.NotFound");
    }

    [Fact]
    public async Task IsRoomAvailableAsync_Should_Return_True_When_Room_Available()
    {
        // Arrange
        var location = new Location { Id = 1, Name = "Spa", City = "City", State = "ST", ZipCode = "12345" };
        var room = new Room { Id = 1, Name = "Room 1", LocationId = 1, IsActive = true, DisplayOrder = 1 };

        await _context.Locations.AddAsync(location);
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IsRoomAvailableAsync(
            room.Id,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1)
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task IsRoomAvailableAsync_Should_Return_False_When_Room_Has_Conflicting_Booking()
    {
        // Arrange
        var location = new Location { Id = 1, Name = "Spa", City = "City", State = "ST", ZipCode = "12345" };
        var room = new Room { Id = 1, Name = "Room 1", LocationId = 1, IsActive = true, DisplayOrder = 1 };
        
        var booking = new Booking
        {
            RoomId = room.Id,
            ClientId = "client-1",
            TherapistId = "therapist-1",
            ServiceId = 1,
            LocationId = 1,
            StartTime = new DateTime(2025, 12, 15, 10, 0, 0),
            EndTime = new DateTime(2025, 12, 15, 11, 0, 0),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Locations.AddAsync(location);
        await _context.Rooms.AddAsync(room);
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Try to book overlapping time
        var requestTime = new DateTime(2025, 12, 15, 10, 30, 0);

        // Act
        var result = await _service.IsRoomAvailableAsync(
            room.Id,
            requestTime,
            requestTime.AddHours(1)
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsRoomAvailableAsync_Should_Consider_Buffer_Time()
    {
        // Arrange
        var location = new Location { Id = 1, Name = "Spa", City = "City", State = "ST", ZipCode = "12345" };
        var room = new Room { Id = 1, Name = "Room 1", LocationId = 1, IsActive = true, DisplayOrder = 1 };
        
        var booking = new Booking
        {
            RoomId = room.Id,
            ClientId = "client-1",
            TherapistId = "therapist-1",
            ServiceId = 1,
            LocationId = 1,
            StartTime = new DateTime(2025, 12, 15, 10, 0, 0),
            EndTime = new DateTime(2025, 12, 15, 11, 0, 0),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Locations.AddAsync(location);
        await _context.Rooms.AddAsync(room);
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Try to book immediately after (should fail due to 15min buffer)
        var requestTime = new DateTime(2025, 12, 15, 11, 0, 0);

        // Act
        var result = await _service.IsRoomAvailableAsync(
            room.Id,
            requestTime,
            requestTime.AddHours(1)
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse(); // Buffer time blocks this slot
    }

    [Fact]
    public async Task IsRoomAvailableAsync_Should_Ignore_Cancelled_Bookings()
    {
        // Arrange
        var location = new Location { Id = 1, Name = "Spa", City = "City", State = "ST", ZipCode = "12345" };
        var room = new Room { Id = 1, Name = "Room 1", LocationId = 1, IsActive = true, DisplayOrder = 1 };
        
        var booking = new Booking
        {
            RoomId = room.Id,
            ClientId = "client-1",
            TherapistId = "therapist-1",
            ServiceId = 1,
            LocationId = 1,
            StartTime = new DateTime(2025, 12, 15, 10, 0, 0),
            EndTime = new DateTime(2025, 12, 15, 11, 0, 0),
            Status = BookingStatus.Cancelled,
            TotalPrice = 100m
        };

        await _context.Locations.AddAsync(location);
        await _context.Rooms.AddAsync(room);
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Try to book same time
        var requestTime = new DateTime(2025, 12, 15, 10, 0, 0);

        // Act
        var result = await _service.IsRoomAvailableAsync(
            room.Id,
            requestTime,
            requestTime.AddHours(1)
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue(); // Cancelled bookings don't block
    }

    [Fact]
    public async Task IsRoomAvailableAsync_Should_Exclude_Specified_Booking()
    {
        // Arrange
        var location = new Location { Id = 1, Name = "Spa", City = "City", State = "ST", ZipCode = "12345" };
        var room = new Room { Id = 1, Name = "Room 1", LocationId = 1, IsActive = true, DisplayOrder = 1 };
        
        var booking = new Booking
        {
            Id = 1,
            RoomId = room.Id,
            ClientId = "client-1",
            TherapistId = "therapist-1",
            ServiceId = 1,
            LocationId = 1,
            StartTime = new DateTime(2025, 12, 15, 10, 0, 0),
            EndTime = new DateTime(2025, 12, 15, 11, 0, 0),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Locations.AddAsync(location);
        await _context.Rooms.AddAsync(room);
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Check same time, excluding this booking
        var requestTime = new DateTime(2025, 12, 15, 10, 0, 0);

        // Act
        var result = await _service.IsRoomAvailableAsync(
            room.Id,
            requestTime,
            requestTime.AddHours(1),
            excludeBookingId: 1
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue(); // Should be available when excluding booking 1
    }

    #endregion

    #region GetAvailableRoomsAsync Tests

    [Fact]
    public async Task GetAvailableRoomsAsync_Should_Return_Only_Available_Rooms()
    {
        // Arrange
        var location = new Location { Id = 1, Name = "Spa", City = "City", State = "ST", ZipCode = "12345" };
        var room1 = new Room { Id = 1, Name = "Room 1", LocationId = 1, IsActive = true, DisplayOrder = 1 };
        var room2 = new Room { Id = 2, Name = "Room 2", LocationId = 1, IsActive = true, DisplayOrder = 2 };
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };

        var capability1 = new RoomServiceCapability { RoomId = 1, ServiceId = 1 };
        var capability2 = new RoomServiceCapability { RoomId = 2, ServiceId = 1 };

        // Room 2 has a booking
        var booking = new Booking
        {
            RoomId = 2,
            ClientId = "client-1",
            TherapistId = "therapist-1",
            ServiceId = 1,
            LocationId = 1,
            StartTime = new DateTime(2025, 12, 15, 10, 0, 0),
            EndTime = new DateTime(2025, 12, 15, 11, 0, 0),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Locations.AddAsync(location);
        await _context.Rooms.AddRangeAsync(room1, room2);
        await _context.SpaServices.AddAsync(service);
        await _context.RoomServiceCapabilities.AddRangeAsync(capability1, capability2);
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        var requestTime = new DateTime(2025, 12, 15, 10, 0, 0);

        // Act
        var availableRooms = await _service.GetAvailableRoomsAsync(
            service.Id,
            requestTime,
            requestTime.AddHours(1)
        );

        // Assert
        availableRooms.Should().HaveCount(1);
        availableRooms[0].Id.Should().Be(room1.Id);
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_Should_Return_Only_Compatible_Rooms()
    {
        // Arrange
        var location = new Location { Id = 1, Name = "Spa", City = "City", State = "ST", ZipCode = "12345" };
        var room1 = new Room { Id = 1, Name = "Room 1", LocationId = 1, IsActive = true, DisplayOrder = 1 };
        var room2 = new Room { Id = 2, Name = "Room 2", LocationId = 1, IsActive = true, DisplayOrder = 2 };
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };

        // Only room1 is compatible with service
        var capability1 = new RoomServiceCapability { RoomId = 1, ServiceId = 1 };

        await _context.Locations.AddAsync(location);
        await _context.Rooms.AddRangeAsync(room1, room2);
        await _context.SpaServices.AddAsync(service);
        await _context.RoomServiceCapabilities.AddAsync(capability1);
        await _context.SaveChangesAsync();

        var requestTime = new DateTime(2025, 12, 15, 10, 0, 0);

        // Act
        var availableRooms = await _service.GetAvailableRoomsAsync(
            service.Id,
            requestTime,
            requestTime.AddHours(1)
        );

        // Assert
        availableRooms.Should().HaveCount(1);
        availableRooms[0].Id.Should().Be(room1.Id);
    }

    #endregion

    #region FindBestAvailableRoomAsync Tests

    [Fact]
    public async Task FindBestAvailableRoomAsync_Should_Return_First_Available_Room_By_DisplayOrder()
    {
        // Arrange
        var location = new Location { Id = 1, Name = "Spa", City = "City", State = "ST", ZipCode = "12345" };
        var room1 = new Room { Id = 1, Name = "Room 1", LocationId = 1, IsActive = true, DisplayOrder = 2 };
        var room2 = new Room { Id = 2, Name = "Room 2", LocationId = 1, IsActive = true, DisplayOrder = 1 };
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };

        var capability1 = new RoomServiceCapability { RoomId = 1, ServiceId = 1 };
        var capability2 = new RoomServiceCapability { RoomId = 2, ServiceId = 1 };

        await _context.Locations.AddAsync(location);
        await _context.Rooms.AddRangeAsync(room1, room2);
        await _context.SpaServices.AddAsync(service);
        await _context.RoomServiceCapabilities.AddRangeAsync(capability1, capability2);
        await _context.SaveChangesAsync();

        var requestTime = new DateTime(2025, 12, 15, 10, 0, 0);

        // Act
        var result = await _service.FindBestAvailableRoomAsync(
            service.Id,
            requestTime,
            requestTime.AddHours(1)
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(room2.Id); // Room 2 has lower DisplayOrder
    }

    [Fact]
    public async Task FindBestAvailableRoomAsync_Should_Fail_When_No_Rooms_Available()
    {
        // Arrange
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };
        await _context.SpaServices.AddAsync(service);
        await _context.SaveChangesAsync();

        var requestTime = new DateTime(2025, 12, 15, 10, 0, 0);

        // Act
        var result = await _service.FindBestAvailableRoomAsync(
            service.Id,
            requestTime,
            requestTime.AddHours(1)
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Room.NotAvailable");
    }

    #endregion

    #region GetRoomsForServiceAsync Tests

    [Fact]
    public async Task GetRoomsForServiceAsync_Should_Return_Compatible_Active_Rooms()
    {
        // Arrange
        var location = new Location { Id = 1, Name = "Spa", City = "City", State = "ST", ZipCode = "12345" };
        var room1 = new Room { Id = 1, Name = "Room 1", LocationId = 1, IsActive = true, DisplayOrder = 1 };
        var room2 = new Room { Id = 2, Name = "Room 2", LocationId = 1, IsActive = false, DisplayOrder = 2 }; // Inactive
        var room3 = new Room { Id = 3, Name = "Room 3", LocationId = 1, IsActive = true, DisplayOrder = 3 };
        var service = new SpaService { Id = 1, Name = "Massage", BasePrice = 100m, IsActive = true, LocationId = 1 };

        var capability1 = new RoomServiceCapability { RoomId = 1, ServiceId = 1 };
        var capability2 = new RoomServiceCapability { RoomId = 2, ServiceId = 1 };
        var capability3 = new RoomServiceCapability { RoomId = 3, ServiceId = 1 };

        await _context.Locations.AddAsync(location);
        await _context.Rooms.AddRangeAsync(room1, room2, room3);
        await _context.SpaServices.AddAsync(service);
        await _context.RoomServiceCapabilities.AddRangeAsync(capability1, capability2, capability3);
        await _context.SaveChangesAsync();

        // Act
        var rooms = await _service.GetRoomsForServiceAsync(service.Id);

        // Assert
        rooms.Should().HaveCount(2); // Only active rooms
        rooms.Should().Contain(r => r.Id == room1.Id);
        rooms.Should().Contain(r => r.Id == room3.Id);
        rooms.Should().NotContain(r => r.Id == room2.Id);
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

