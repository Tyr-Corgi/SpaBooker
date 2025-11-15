using FluentAssertions;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;

namespace SpaBooker.Tests.Unit.Entities;

public class UserMembershipTests
{
    [Fact]
    public void UserMembership_Should_Create_With_Valid_Properties()
    {
        // Arrange
        var userId = "user-123";
        var membershipPlanId = 1;
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddMonths(1);

        // Act
        var membership = new UserMembership
        {
            UserId = userId,
            MembershipPlanId = membershipPlanId,
            Status = MembershipStatus.Active,
            StartDate = startDate,
            EndDate = endDate,
            CurrentCredits = 4.0m
        };

        // Assert
        membership.UserId.Should().Be(userId);
        membership.MembershipPlanId.Should().Be(membershipPlanId);
        membership.Status.Should().Be(MembershipStatus.Active);
        membership.StartDate.Should().Be(startDate);
        membership.EndDate.Should().Be(endDate);
        membership.CurrentCredits.Should().Be(4.0m);
    }

    [Fact]
    public void UserMembership_Credits_Should_Decrease_After_Use()
    {
        // Arrange
        var membership = new UserMembership
        {
            CurrentCredits = 4.0m
        };

        // Act
        membership.CurrentCredits -= 1.0m;

        // Assert
        membership.CurrentCredits.Should().Be(3.0m);
    }

    [Theory]
    [InlineData(MembershipStatus.Active)]
    [InlineData(MembershipStatus.Cancelled)]
    [InlineData(MembershipStatus.Expired)]
    [InlineData(MembershipStatus.PastDue)]
    [InlineData(MembershipStatus.Inactive)]
    public void UserMembership_Should_Accept_All_Valid_Statuses(MembershipStatus status)
    {
        // Arrange & Act
        var membership = new UserMembership
        {
            Status = status
        };

        // Assert
        membership.Status.Should().Be(status);
    }

    [Fact]
    public void UserMembership_Should_Track_Next_Billing_Date()
    {
        // Arrange
        var nextBillingDate = DateTime.UtcNow.AddMonths(1);

        // Act
        var membership = new UserMembership
        {
            NextBillingDate = nextBillingDate
        };

        // Assert
        membership.NextBillingDate.Should().Be(nextBillingDate);
    }

    [Fact]
    public void UserMembership_Should_Calculate_If_Active()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var activeMembership = new UserMembership
        {
            Status = MembershipStatus.Active,
            StartDate = now.AddDays(-10),
            EndDate = now.AddDays(20)
        };

        var expiredMembership = new UserMembership
        {
            Status = MembershipStatus.Expired,
            StartDate = now.AddDays(-60),
            EndDate = now.AddDays(-30)
        };

        // Assert
        activeMembership.Status.Should().Be(MembershipStatus.Active);
        expiredMembership.Status.Should().Be(MembershipStatus.Expired);
    }

    [Fact]
    public void UserMembership_Should_Track_Stripe_Subscription()
    {
        // Arrange
        var stripeSubscriptionId = "sub_123456";
        var stripeCustomerId = "cus_789012";

        // Act
        var membership = new UserMembership
        {
            StripeSubscriptionId = stripeSubscriptionId,
            StripeCustomerId = stripeCustomerId
        };

        // Assert
        membership.StripeSubscriptionId.Should().Be(stripeSubscriptionId);
        membership.StripeCustomerId.Should().Be(stripeCustomerId);
    }

    [Fact]
    public void UserMembership_Should_Have_Credit_Transactions()
    {
        // Arrange & Act
        var membership = new UserMembership
        {
            CreditTransactions = new List<MembershipCreditTransaction>()
        };

        // Assert
        membership.CreditTransactions.Should().NotBeNull();
        membership.CreditTransactions.Should().BeEmpty();
    }

    [Fact]
    public void UserMembership_Should_Track_Creation_And_Updates()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var membership = new UserMembership
        {
            CreatedAt = now,
            UpdatedAt = now.AddMinutes(5)
        };

        // Assert
        membership.CreatedAt.Should().Be(now);
        membership.UpdatedAt.Should().Be(now.AddMinutes(5));
    }
}
