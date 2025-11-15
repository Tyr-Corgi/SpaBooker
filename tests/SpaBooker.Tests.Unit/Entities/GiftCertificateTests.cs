using FluentAssertions;
using SpaBooker.Core.Entities;

namespace SpaBooker.Tests.Unit.Entities;

public class GiftCertificateTests
{
    [Fact]
    public void GiftCertificate_Should_Create_With_Valid_Properties()
    {
        // Arrange
        var code = "GIFT-2025-ABCD";
        var originalAmount = 150.00m;
        var expiresAt = DateTime.UtcNow.AddYears(1);

        // Act
        var giftCert = new GiftCertificate
        {
            Code = code,
            OriginalAmount = originalAmount,
            RemainingBalance = originalAmount,
            IsActive = true,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        giftCert.Code.Should().Be(code);
        giftCert.OriginalAmount.Should().Be(originalAmount);
        giftCert.RemainingBalance.Should().Be(originalAmount);
        giftCert.IsActive.Should().BeTrue();
        giftCert.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void GiftCertificate_Balance_Should_Decrease_After_Use()
    {
        // Arrange
        var originalAmount = 100.00m;
        var giftCert = new GiftCertificate
        {
            OriginalAmount = originalAmount,
            RemainingBalance = originalAmount
        };
        var amountUsed = 30.00m;

        // Act
        giftCert.RemainingBalance -= amountUsed;

        // Assert
        giftCert.RemainingBalance.Should().Be(70.00m);
    }

    [Fact]
    public void GiftCertificate_Should_Be_Deactivatable()
    {
        // Arrange
        var giftCert = new GiftCertificate
        {
            IsActive = true,
            Status = "Active"
        };

        // Act
        giftCert.IsActive = false;
        giftCert.Status = "Cancelled";

        // Assert
        giftCert.IsActive.Should().BeFalse();
        giftCert.Status.Should().Be("Cancelled");
    }

    [Fact]
    public void GiftCertificate_Should_Support_Recipient_Information()
    {
        // Arrange
        var recipientEmail = "recipient@example.com";
        var recipientName = "John Doe";
        var personalMessage = "Happy Birthday!";

        // Act
        var giftCert = new GiftCertificate
        {
            RecipientEmail = recipientEmail,
            RecipientName = recipientName,
            PersonalMessage = personalMessage
        };

        // Assert
        giftCert.RecipientEmail.Should().Be(recipientEmail);
        giftCert.RecipientName.Should().Be(recipientName);
        giftCert.PersonalMessage.Should().Be(personalMessage);
    }

    [Fact]
    public void GiftCertificate_Code_Should_Be_Unique_Identifier()
    {
        // Arrange & Act
        var giftCert1 = new GiftCertificate { Code = "GIFT-001" };
        var giftCert2 = new GiftCertificate { Code = "GIFT-002" };

        // Assert
        giftCert1.Code.Should().NotBe(giftCert2.Code);
    }

    [Fact]
    public void GiftCertificate_Should_Track_Purchaser()
    {
        // Arrange
        var purchaserId = "user-123";
        var purchasePrice = 100.00m;

        // Act
        var giftCert = new GiftCertificate
        {
            PurchasedByUserId = purchaserId,
            PurchasePrice = purchasePrice,
            PurchasedAt = DateTime.UtcNow
        };

        // Assert
        giftCert.PurchasedByUserId.Should().Be(purchaserId);
        giftCert.PurchasePrice.Should().Be(purchasePrice);
    }

    [Fact]
    public void GiftCertificate_Should_Track_Redemption_Status()
    {
        // Arrange
        var giftCert = new GiftCertificate
        {
            IsRedeemed = false
        };
        var redeemedByUserId = "user-456";

        // Act
        giftCert.IsRedeemed = true;
        giftCert.RedeemedByUserId = redeemedByUserId;
        giftCert.RedeemedAt = DateTime.UtcNow;

        // Assert
        giftCert.IsRedeemed.Should().BeTrue();
        giftCert.RedeemedByUserId.Should().Be(redeemedByUserId);
        giftCert.RedeemedAt.Should().NotBeNull();
    }

    [Fact]
    public void GiftCertificate_Should_Support_Location_Restrictions()
    {
        // Arrange
        var locationId = 5;

        // Act
        var giftCert = new GiftCertificate
        {
            RestrictedToLocationId = locationId
        };

        // Assert
        giftCert.RestrictedToLocationId.Should().Be(locationId);
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("PartiallyUsed")]
    [InlineData("FullyRedeemed")]
    [InlineData("Expired")]
    [InlineData("Cancelled")]
    public void GiftCertificate_Should_Accept_All_Valid_Statuses(string status)
    {
        // Arrange & Act
        var giftCert = new GiftCertificate
        {
            Status = status
        };

        // Assert
        giftCert.Status.Should().Be(status);
    }
}
