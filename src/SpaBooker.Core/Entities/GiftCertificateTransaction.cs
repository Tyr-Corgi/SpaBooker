namespace SpaBooker.Core.Entities;

public class GiftCertificateTransaction
{
    public int Id { get; set; }
    
    public int GiftCertificateId { get; set; }
    public GiftCertificate GiftCertificate { get; set; } = null!;
    
    public string Type { get; set; } = string.Empty; // "Purchase", "Redemption", "Refund", "Adjustment", "Expiration"
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    // Related Booking (if redeemed for a booking)
    public int? RelatedBookingId { get; set; }
    public Booking? RelatedBooking { get; set; }
    
    // Who performed the transaction
    public string? PerformedByUserId { get; set; }
    public ApplicationUser? PerformedByUser { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

