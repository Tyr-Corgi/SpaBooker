using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;
using SpaBooker.Core.Interfaces;
using SpaBooker.Core.Settings;
using SpaBooker.Infrastructure.Data;
using SpaBooker.Infrastructure.Services;

namespace SpaBooker.Tests.Integration.Services;

public class MembershipCreditServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly MembershipCreditService _service;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<MembershipCreditService>> _loggerMock;

    public MembershipCreditServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<MembershipCreditService>>();
        
        // Setup UnitOfWork mock to return a mock transaction
        var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);
        
        // When CommitAsync is called, actually save the context changes
        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(async () => await _context.SaveChangesAsync());
        
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(async () => await _context.SaveChangesAsync());
        
        var membershipSettings = new MembershipSettings
        {
            CreditExpirationMonths = 12,
            AllowUnlimitedRollover = true
        };
        var optionsWrapper = Options.Create(membershipSettings);
        
        _service = new MembershipCreditService(_context, _unitOfWorkMock.Object, _loggerMock.Object, optionsWrapper);
    }

    [Fact]
    public async Task AddMonthlyCredits_Should_Increase_Current_Credits()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-1", UserName = "testuser", Email = "test@example.com" };
        var plan = new MembershipPlan 
        { 
            Id = 1, 
            Name = "Monthly", 
            MonthlyCredits = 4, 
            MonthlyPrice = 99.99m 
        };
        var membership = new UserMembership
        {
            Id = 1,
            UserId = user.Id,
            MembershipPlanId = plan.Id,
            Status = MembershipStatus.Active,
            CurrentCredits = 4.0m,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(20)
        };

        await _context.Users.AddAsync(user);
        await _context.MembershipPlans.AddAsync(plan);
        await _context.UserMemberships.AddAsync(membership);
        await _context.SaveChangesAsync();

        // Act
        await _service.AddMonthlyCreditsAsync(membership.Id, 4.0m, "Monthly credit refresh");

        // Assert
        var updatedMembership = await _context.UserMemberships.FindAsync(membership.Id);
        updatedMembership!.CurrentCredits.Should().Be(8.0m);
    }

    [Fact]
    public async Task DeductCredits_Should_Decrease_Current_Credits()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-1", UserName = "testuser", Email = "test@example.com" };
        var plan = new MembershipPlan 
        { 
            Id = 1, 
            Name = "Monthly", 
            MonthlyCredits = 4, 
            MonthlyPrice = 99.99m 
        };
        var membership = new UserMembership
        {
            Id = 1,
            UserId = user.Id,
            MembershipPlanId = plan.Id,
            Status = MembershipStatus.Active,
            CurrentCredits = 4.0m,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(20)
        };

        await _context.Users.AddAsync(user);
        await _context.MembershipPlans.AddAsync(plan);
        await _context.UserMemberships.AddAsync(membership);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeductCreditsAsync(membership.Id, 1.0m, "Used for booking");

        // Assert
        var updatedMembership = await _context.UserMemberships.FindAsync(membership.Id);
        updatedMembership!.CurrentCredits.Should().Be(3.0m);
    }

    [Fact]
    public async Task GetAvailableCredits_Should_Return_Correct_Count()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-1", UserName = "testuser", Email = "test@example.com" };
        var plan = new MembershipPlan 
        { 
            Id = 1, 
            Name = "Monthly", 
            MonthlyCredits = 4, 
            MonthlyPrice = 99.99m 
        };
        var membership = new UserMembership
        {
            Id = 1,
            UserId = user.Id,
            MembershipPlanId = plan.Id,
            Status = MembershipStatus.Active,
            CurrentCredits = 3.0m,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(20)
        };

        await _context.Users.AddAsync(user);
        await _context.MembershipPlans.AddAsync(plan);
        await _context.UserMemberships.AddAsync(membership);
        await _context.SaveChangesAsync();

        // Act
        var credits = await _service.GetAvailableCreditsAsync(membership.Id);

        // Assert
        credits.Should().Be(3.0m);
    }

    [Fact]
    public async Task GetAvailableCredits_Should_Return_Zero_For_NonExistent_Membership()
    {
        // Arrange
        var membershipId = 999;

        // Act
        var credits = await _service.GetAvailableCreditsAsync(membershipId);

        // Assert
        credits.Should().Be(0m);
    }

    [Fact]
    public async Task AddMonthlyCredits_Should_Create_Transaction_Record()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-1", UserName = "testuser", Email = "test@example.com" };
        var plan = new MembershipPlan 
        { 
            Id = 1, 
            Name = "Monthly", 
            MonthlyCredits = 4, 
            MonthlyPrice = 99.99m 
        };
        var membership = new UserMembership
        {
            Id = 1,
            UserId = user.Id,
            MembershipPlanId = plan.Id,
            Status = MembershipStatus.Active,
            CurrentCredits = 2.0m,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(20)
        };

        await _context.Users.AddAsync(user);
        await _context.MembershipPlans.AddAsync(plan);
        await _context.UserMemberships.AddAsync(membership);
        await _context.SaveChangesAsync();

        // Act
        await _service.AddMonthlyCreditsAsync(membership.Id, 4.0m, "Monthly refresh");

        // Assert
        var transactions = await _context.MembershipCreditTransactions
            .Where(t => t.UserMembershipId == membership.Id)
            .ToListAsync();
        
        transactions.Should().HaveCount(1);
        transactions[0].Amount.Should().Be(4.0m);
        transactions[0].Type.Should().Be("Credit");
        transactions[0].ExpiresAt.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
