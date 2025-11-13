namespace SpaBooker.Core.Entities;

public class MembershipCreditTransaction
{
    public int Id { get; set; }
    public int UserMembershipId { get; set; }
    public UserMembership UserMembership { get; set; } = null!;
    
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // "Credit", "Debit", "Expired", "Adjusted"
    public string Description { get; set; } = string.Empty;
    public int? RelatedBookingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; } // Credits expire 12 months from creation
}

