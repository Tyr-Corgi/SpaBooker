using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Interfaces;
using SpaBooker.Core.Settings;
using SpaBooker.Infrastructure.Data;
using SpaBooker.Infrastructure.Services;

namespace SpaBooker.Tests.Integration.Services;

public class GiftCertificateServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly GiftCertificateService _service;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<GiftCertificateService>> _loggerMock;

    public GiftCertificateServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _emailServiceMock = new Mock<IEmailService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<GiftCertificateService>>();

        _service = new GiftCertificateService(_context, _unitOfWorkMock.Object, _emailServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateGiftCertificate_Should_Create_With_Unique_Code()
    {
        // Arrange
        var purchaserId = "user-1";
        var amount = 100.00m;
        var recipientEmail = "recipient@example.com";
        var recipientName = "Jane Doe";
        var message = "Happy Birthday!";

        // Act
        var giftCert = await _service.CreateGiftCertificateAsync(
            purchaserId, amount, recipientName, recipientEmail, null, message);

        // Assert
        giftCert.Should().NotBeNull();
        giftCert.Code.Should().NotBeNullOrEmpty();
        giftCert.OriginalAmount.Should().Be(amount);
        giftCert.RemainingBalance.Should().Be(amount);
        giftCert.RecipientEmail.Should().Be(recipientEmail);
        giftCert.RecipientName.Should().Be(recipientName);
        giftCert.PersonalMessage.Should().Be(message);
        giftCert.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetGiftCertificateByCode_Should_Return_Certificate()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-1", UserName = "testuser", Email = "test@example.com" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var giftCert = new GiftCertificate
        {
            Code = "TEST-GIFT-123", // Will be stored as uppercase in real implementation
            OriginalAmount = 50.00m,
            RemainingBalance = 50.00m,
            PurchasedByUserId = user.Id,
            RecipientName = "Test User",
            RecipientEmail = "test@example.com",
            IsActive = true,
            Status = "Active",
            PurchasePrice = 50.00m,
            PurchasedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow
        };
        // The service expects uppercase codes
        giftCert.Code = giftCert.Code.ToUpper();
        await _context.GiftCertificates.AddAsync(giftCert);
        await _context.SaveChangesAsync();

        // Act - service will convert to uppercase
        var result = await _service.GetGiftCertificateByCodeAsync("test-gift-123");

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("TEST-GIFT-123");
        result.RemainingBalance.Should().Be(50.00m);
    }

    [Fact]
    public async Task GetGiftCertificateByCode_Should_Return_Null_For_Invalid_Code()
    {
        // Act
        var result = await _service.GetGiftCertificateByCodeAsync("INVALID-CODE");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateGiftCertificate_Should_Return_True_For_Valid_Certificate()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-1", UserName = "testuser", Email = "test@example.com" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var giftCert = new GiftCertificate
        {
            Code = "VALID-CERT", // Already uppercase
            OriginalAmount = 100.00m,
            RemainingBalance = 75.00m,
            PurchasedByUserId = user.Id,
            RecipientName = "Test User",
            RecipientEmail = "test@example.com",
            IsActive = true,
            Status = "Active",
            PurchasePrice = 100.00m,
            PurchasedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMonths(6),
            CreatedAt = DateTime.UtcNow
        };
        await _context.GiftCertificates.AddAsync(giftCert);
        await _context.SaveChangesAsync();

        // Act - the service will convert to uppercase
        var result = await _service.ValidateGiftCertificateAsync("valid-cert");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateGiftCertificate_Should_Return_False_For_Expired_Certificate()
    {
        // Arrange
        var giftCert = new GiftCertificate
        {
            Code = "EXPIRED-CERT",
            OriginalAmount = 100.00m,
            RemainingBalance = 100.00m,
            PurchasedByUserId = "user-1",
            RecipientName = "Test User",
            RecipientEmail = "test@example.com",
            IsActive = true,
            Status = "Active",
            PurchasePrice = 100.00m,
            PurchasedAt = DateTime.UtcNow.AddMonths(-13),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddMonths(-13)
        };
        await _context.GiftCertificates.AddAsync(giftCert);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ValidateGiftCertificateAsync("EXPIRED-CERT");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateGiftCertificate_Should_Return_False_For_Inactive_Certificate()
    {
        // Arrange
        var giftCert = new GiftCertificate
        {
            Code = "INACTIVE-CERT",
            OriginalAmount = 100.00m,
            RemainingBalance = 100.00m,
            PurchasedByUserId = "user-1",
            RecipientName = "Test User",
            RecipientEmail = "test@example.com",
            IsActive = false,
            Status = "Cancelled",
            PurchasePrice = 100.00m,
            PurchasedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow
        };
        await _context.GiftCertificates.AddAsync(giftCert);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ValidateGiftCertificateAsync("INACTIVE-CERT");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RedeemGiftCertificate_Should_Reduce_Balance()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-1", UserName = "testuser", Email = "test@example.com" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var giftCert = new GiftCertificate
        {
            Code = "REDEEM-TEST", // Already uppercase
            OriginalAmount = 100.00m,
            RemainingBalance = 100.00m,
            PurchasedByUserId = user.Id,
            RecipientName = "Test User",
            RecipientEmail = "test@example.com",
            IsActive = true,
            Status = "Active",
            PurchasePrice = 100.00m,
            PurchasedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow
        };
        await _context.GiftCertificates.AddAsync(giftCert);
        await _context.SaveChangesAsync();

        // Act - service will convert to uppercase
        var amountRedeemed = await _service.RedeemGiftCertificateAsync("redeem-test", "user-2", 75.00m);

        // Assert
        amountRedeemed.Should().Be(75.00m);
        var updated = await _context.GiftCertificates.FirstAsync(g => g.Code == "REDEEM-TEST");
        updated.RemainingBalance.Should().Be(25.00m);
    }

    [Fact]
    public async Task GetPurchasedGiftCertificates_Should_Return_User_Certificates()
    {
        // Arrange
        var userId = "user-1";
        var giftCert1 = new GiftCertificate
        {
            Code = "USER-CERT-1",
            PurchasedByUserId = userId,
            OriginalAmount = 50.00m,
            RemainingBalance = 50.00m,
            RecipientName = "Test1",
            RecipientEmail = "test1@example.com",
            IsActive = true,
            Status = "Active",
            PurchasePrice = 50.00m,
            PurchasedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow
        };
        var giftCert2 = new GiftCertificate
        {
            Code = "USER-CERT-2",
            PurchasedByUserId = userId,
            OriginalAmount = 100.00m,
            RemainingBalance = 75.00m,
            RecipientName = "Test2",
            RecipientEmail = "test2@example.com",
            IsActive = true,
            Status = "Active",
            PurchasePrice = 100.00m,
            PurchasedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow
        };
        var otherUserCert = new GiftCertificate
        {
            Code = "OTHER-CERT",
            PurchasedByUserId = "user-999",
            OriginalAmount = 25.00m,
            RemainingBalance = 25.00m,
            RecipientName = "Test3",
            RecipientEmail = "test3@example.com",
            IsActive = true,
            Status = "Active",
            PurchasePrice = 25.00m,
            PurchasedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow
        };

        await _context.GiftCertificates.AddRangeAsync(giftCert1, giftCert2, otherUserCert);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetPurchasedGiftCertificatesAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(cert => cert.PurchasedByUserId.Should().Be(userId));
    }

    [Fact]
    public async Task GenerateUniqueCode_Should_Create_Unique_Code()
    {
        // Act
        var code1 = await _service.GenerateUniqueCodeAsync();
        var code2 = await _service.GenerateUniqueCodeAsync();

        // Assert
        code1.Should().NotBeNullOrEmpty();
        code2.Should().NotBeNullOrEmpty();
        code1.Should().NotBe(code2);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

