namespace SpaBooker.Core.Entities;

public class MembershipPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal MonthlyCredits { get; set; }
    public decimal DiscountPercentage { get; set; }
    public bool IsActive { get; set; } = true;
    public string? StripePriceId { get; set; }
    public string? StripeProductId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<UserMembership> UserMemberships { get; set; } = new List<UserMembership>();
}

