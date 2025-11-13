using SpaBooker.Core.Enums;

namespace SpaBooker.Core.Entities;

public class UserMembership
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public int MembershipPlanId { get; set; }
    public MembershipPlan MembershipPlan { get; set; } = null!;
    
    public MembershipStatus Status { get; set; } = MembershipStatus.Active;
    public decimal CurrentCredits { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    
    public string? StripeSubscriptionId { get; set; }
    public string? StripeCustomerId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<MembershipCreditTransaction> CreditTransactions { get; set; } = new List<MembershipCreditTransaction>();
}

