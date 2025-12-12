using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;
using SpaBooker.Core.Interfaces;
using SpaBooker.Infrastructure.Data;
using SpaBooker.Infrastructure.Services;

namespace SpaBooker.Tests.Integration.Services;

public class ClientServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ClientService _service;
    private readonly Mock<IAuditService> _auditServiceMock;

    public ClientServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _auditServiceMock = new Mock<IAuditService>();

        _service = new ClientService(_context, _auditServiceMock.Object);
    }

    #region GetClientByIdAsync Tests

    [Fact]
    public async Task GetClientByIdAsync_Should_Return_Client_When_Found()
    {
        // Arrange
        var client = new ApplicationUser
        {
            Id = "client-1",
            UserName = "client",
            Email = "client@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        await _context.Users.AddAsync(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetClientByIdAsync(client.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(client.Id);
        result.Value.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetClientByIdAsync_Should_Fail_When_Client_Not_Found()
    {
        // Act
        var result = await _service.GetClientByIdAsync("non-existent");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Client.NotFound");
    }

    #endregion

    #region GetClientStatisticsAsync Tests

    [Fact]
    public async Task GetClientStatisticsAsync_Should_Return_Correct_Statistics()
    {
        // Arrange
        var client = new ApplicationUser
        {
            Id = "client-1",
            UserName = "client",
            Email = "client@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var therapist = new ApplicationUser
        {
            Id = "therapist-1",
            UserName = "therapist",
            Email = "therapist@test.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var service = new SpaService
        {
            Id = 1,
            Name = "Massage",
            BasePrice = 100m,
            IsActive = true,
            LocationId = 1
        };

        var booking1 = new Booking
        {
            ClientId = client.Id,
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(-30),
            EndTime = DateTime.UtcNow.AddDays(-30).AddHours(1),
            Status = BookingStatus.Completed,
            TotalPrice = 100m
        };

        var booking2 = new Booking
        {
            ClientId = client.Id,
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(-15),
            EndTime = DateTime.UtcNow.AddDays(-15).AddHours(1),
            Status = BookingStatus.Completed,
            TotalPrice = 100m
        };

        var booking3 = new Booking
        {
            ClientId = client.Id,
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(-5),
            EndTime = DateTime.UtcNow.AddDays(-5).AddHours(1),
            Status = BookingStatus.Cancelled,
            TotalPrice = 100m
        };

        await _context.Users.AddRangeAsync(client, therapist);
        await _context.SpaServices.AddAsync(service);
        await _context.Bookings.AddRangeAsync(booking1, booking2, booking3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetClientStatisticsAsync(client.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalBookings.Should().Be(3);
        result.Value.CompletedBookings.Should().Be(2);
        result.Value.CancelledBookings.Should().Be(1);
        result.Value.LifetimeValue.Should().Be(200m); // Only completed bookings
        result.Value.FavoriteService.Should().Be("Massage");
        result.Value.FavoriteTherapist.Should().Be("Jane Smith");
    }

    [Fact]
    public async Task GetClientStatisticsAsync_Should_Calculate_Days_Since_Last_Visit()
    {
        // Arrange
        var client = new ApplicationUser
        {
            Id = "client-1",
            UserName = "client",
            Email = "client@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var therapist = new ApplicationUser
        {
            Id = "therapist-1",
            UserName = "therapist",
            Email = "therapist@test.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var service = new SpaService
        {
            Id = 1,
            Name = "Massage",
            BasePrice = 100m,
            IsActive = true,
            LocationId = 1
        };

        var booking = new Booking
        {
            ClientId = client.Id,
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(-10),
            EndTime = DateTime.UtcNow.AddDays(-10).AddHours(1),
            Status = BookingStatus.Completed,
            TotalPrice = 100m
        };

        await _context.Users.AddRangeAsync(client, therapist);
        await _context.SpaServices.AddAsync(service);
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetClientStatisticsAsync(client.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.DaysSinceLastVisit.Should().BeGreaterThanOrEqualTo(9);
        result.Value.DaysSinceLastVisit.Should().BeLessThanOrEqualTo(11);
    }

    [Fact]
    public async Task GetClientStatisticsAsync_Should_Include_Active_Membership_Info()
    {
        // Arrange
        var client = new ApplicationUser
        {
            Id = "client-1",
            UserName = "client",
            Email = "client@test.com"
        };

        var plan = new MembershipPlan
        {
            Id = 1,
            Name = "Gold",
            MonthlyPrice = 199m,
            MonthlyCredits = 300
        };

        var membership = new UserMembership
        {
            UserId = client.Id,
            MembershipPlanId = plan.Id,
            Status = MembershipStatus.Active,
            CurrentCredits = 250m,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(330)
        };

        await _context.Users.AddAsync(client);
        await _context.MembershipPlans.AddAsync(plan);
        await _context.UserMemberships.AddAsync(membership);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetClientStatisticsAsync(client.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.HasActiveMembership.Should().BeTrue();
        result.Value.MembershipPlanName.Should().Be("Gold");
        result.Value.CurrentCredits.Should().Be(250m);
    }

    [Fact]
    public async Task GetClientStatisticsAsync_Should_Fail_When_Client_Not_Found()
    {
        // Act
        var result = await _service.GetClientStatisticsAsync("non-existent");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Client.NotFound");
    }

    #endregion

    #region GetClientBookingHistoryAsync Tests

    [Fact]
    public async Task GetClientBookingHistoryAsync_Should_Return_Bookings_Ordered_By_Date()
    {
        // Arrange
        var client = new ApplicationUser
        {
            Id = "client-1",
            UserName = "client",
            Email = "client@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var therapist = new ApplicationUser
        {
            Id = "therapist-1",
            UserName = "therapist",
            Email = "therapist@test.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var service = new SpaService
        {
            Id = 1,
            Name = "Massage",
            BasePrice = 100m,
            IsActive = true,
            LocationId = 1
        };

        var room = new Room
        {
            Id = 1,
            Name = "Room 1",
            LocationId = 1,
            IsActive = true,
            DisplayOrder = 1
        };

        var booking1 = new Booking
        {
            Id = 1,
            ClientId = client.Id,
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            RoomId = room.Id,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(-30),
            EndTime = DateTime.UtcNow.AddDays(-30).AddHours(1),
            Status = BookingStatus.Completed,
            TotalPrice = 100m
        };

        var booking2 = new Booking
        {
            Id = 2,
            ClientId = client.Id,
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            RoomId = room.Id,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(-10),
            EndTime = DateTime.UtcNow.AddDays(-10).AddHours(1),
            Status = BookingStatus.Completed,
            TotalPrice = 100m
        };

        var booking3 = new Booking
        {
            Id = 3,
            ClientId = client.Id,
            TherapistId = therapist.Id,
            ServiceId = service.Id,
            RoomId = room.Id,
            LocationId = 1,
            StartTime = DateTime.UtcNow.AddDays(5),
            EndTime = DateTime.UtcNow.AddDays(5).AddHours(1),
            Status = BookingStatus.Confirmed,
            TotalPrice = 100m
        };

        await _context.Users.AddRangeAsync(client, therapist);
        await _context.SpaServices.AddAsync(service);
        await _context.Rooms.AddAsync(room);
        await _context.Bookings.AddRangeAsync(booking1, booking2, booking3);
        await _context.SaveChangesAsync();

        // Act
        var bookings = await _service.GetClientBookingHistoryAsync(client.Id);

        // Assert
        bookings.Should().HaveCount(3);
        bookings[0].Id.Should().Be(3); // Future booking first (most recent)
        bookings[1].Id.Should().Be(2);
        bookings[2].Id.Should().Be(1); // Oldest booking last
    }

    [Fact]
    public async Task GetClientBookingHistoryAsync_Should_Limit_Results_When_Take_Specified()
    {
        // Arrange
        var client = new ApplicationUser
        {
            Id = "client-1",
            UserName = "client",
            Email = "client@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var therapist = new ApplicationUser
        {
            Id = "therapist-1",
            UserName = "therapist",
            Email = "therapist@test.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var service = new SpaService
        {
            Id = 1,
            Name = "Massage",
            BasePrice = 100m,
            IsActive = true,
            LocationId = 1
        };

        var room = new Room
        {
            Id = 1,
            Name = "Room 1",
            LocationId = 1,
            IsActive = true,
            DisplayOrder = 1
        };

        var bookings = new List<Booking>();
        for (int i = 1; i <= 10; i++)
        {
            bookings.Add(new Booking
            {
                ClientId = client.Id,
                TherapistId = therapist.Id,
                ServiceId = service.Id,
                RoomId = room.Id,
                LocationId = 1,
                StartTime = DateTime.UtcNow.AddDays(-i),
                EndTime = DateTime.UtcNow.AddDays(-i).AddHours(1),
                Status = BookingStatus.Completed,
                TotalPrice = 100m
            });
        }

        await _context.Users.AddRangeAsync(client, therapist);
        await _context.SpaServices.AddAsync(service);
        await _context.Rooms.AddAsync(room);
        await _context.Bookings.AddRangeAsync(bookings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetClientBookingHistoryAsync(client.Id, take: 5);

        // Assert
        result.Should().HaveCount(5);
    }

    #endregion

    #region AddClientNoteAsync Tests

    [Fact]
    public async Task AddClientNoteAsync_Should_Create_Note()
    {
        // Arrange
        var client = new ApplicationUser
        {
            Id = "client-1",
            UserName = "client",
            Email = "client@test.com"
        };

        await _context.Users.AddAsync(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AddClientNoteAsync(
            client.Id,
            "Client prefers morning appointments",
            "Preference",
            "Admin User"
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ClientId.Should().Be(client.Id);
        result.Value.Content.Should().Be("Client prefers morning appointments");
        result.Value.NoteType.Should().Be("Preference");

        var savedNote = await _context.ClientNotes.FirstOrDefaultAsync();
        savedNote.Should().NotBeNull();
    }

    [Fact]
    public async Task AddClientNoteAsync_Should_Fail_When_Client_Not_Found()
    {
        // Act
        var result = await _service.AddClientNoteAsync(
            "non-existent",
            "Note content",
            "General",
            "Admin User"
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Client.NotFound");
    }

    #endregion

    #region UpdateClientAsync Tests

    [Fact]
    public async Task UpdateClientAsync_Should_Update_Client_Info()
    {
        // Arrange
        var client = new ApplicationUser
        {
            Id = "client-1",
            UserName = "client",
            Email = "client@test.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "555-1234"
        };

        await _context.Users.AddAsync(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.UpdateClientAsync(
            client.Id,
            "Jane",
            "Smith",
            "555-5678"
        );

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedClient = await _context.Users.FindAsync(client.Id);
        updatedClient!.FirstName.Should().Be("Jane");
        updatedClient.LastName.Should().Be("Smith");
        updatedClient.PhoneNumber.Should().Be("555-5678");
    }

    [Fact]
    public async Task UpdateClientAsync_Should_Fail_When_Client_Not_Found()
    {
        // Act
        var result = await _service.UpdateClientAsync(
            "non-existent",
            "Jane",
            "Smith",
            "555-5678"
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Client.NotFound");
    }

    #endregion

    #region GetClientActiveMembershipAsync Tests

    [Fact]
    public async Task GetClientActiveMembershipAsync_Should_Return_Active_Membership()
    {
        // Arrange
        var client = new ApplicationUser
        {
            Id = "client-1",
            UserName = "client",
            Email = "client@test.com"
        };

        var plan = new MembershipPlan
        {
            Id = 1,
            Name = "Gold",
            MonthlyPrice = 199m,
            MonthlyCredits = 300
        };

        var activeMembership = new UserMembership
        {
            UserId = client.Id,
            MembershipPlanId = plan.Id,
            Status = MembershipStatus.Active,
            CurrentCredits = 250m,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(330)
        };

        var inactiveMembership = new UserMembership
        {
            UserId = client.Id,
            MembershipPlanId = plan.Id,
            Status = MembershipStatus.Cancelled,
            CurrentCredits = 0m,
            StartDate = DateTime.UtcNow.AddDays(-400),
            EndDate = DateTime.UtcNow.AddDays(-30)
        };

        await _context.Users.AddAsync(client);
        await _context.MembershipPlans.AddAsync(plan);
        await _context.UserMemberships.AddRangeAsync(activeMembership, inactiveMembership);
        await _context.SaveChangesAsync();

        // Act
        var membership = await _service.GetClientActiveMembershipAsync(client.Id);

        // Assert
        membership.Should().NotBeNull();
        membership!.Status.Should().Be(MembershipStatus.Active);
        membership.CurrentCredits.Should().Be(250m);
    }

    [Fact]
    public async Task GetClientActiveMembershipAsync_Should_Return_Null_When_No_Active_Membership()
    {
        // Arrange
        var client = new ApplicationUser
        {
            Id = "client-1",
            UserName = "client",
            Email = "client@test.com"
        };

        await _context.Users.AddAsync(client);
        await _context.SaveChangesAsync();

        // Act
        var membership = await _service.GetClientActiveMembershipAsync(client.Id);

        // Assert
        membership.Should().BeNull();
    }

    #endregion

    #region GetClientNotesAsync Tests

    [Fact]
    public async Task GetClientNotesAsync_Should_Return_Notes_Ordered_By_Date()
    {
        // Arrange
        var client = new ApplicationUser
        {
            Id = "client-1",
            UserName = "client",
            Email = "client@test.com"
        };

        var note1 = new ClientNote
        {
            ClientId = client.Id,
            Content = "Old note",
            NoteType = "General",
            CreatedByName = "Admin",
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var note2 = new ClientNote
        {
            ClientId = client.Id,
            Content = "Recent note",
            NoteType = "General",
            CreatedByName = "Admin",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        await _context.Users.AddAsync(client);
        await _context.ClientNotes.AddRangeAsync(note1, note2);
        await _context.SaveChangesAsync();

        // Act
        var notes = await _service.GetClientNotesAsync(client.Id);

        // Assert
        notes.Should().HaveCount(2);
        notes[0].Content.Should().Be("Recent note"); // Most recent first
        notes[1].Content.Should().Be("Old note");
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

