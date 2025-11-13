namespace SpaBooker.Core.Entities;

public class GiftCertificate
{
    public int Id { get; set; }
    
    // Gift Certificate Details
    public string Code { get; set; } = string.Empty; // Unique redemption code (e.g., "GIFT-XXXX-XXXX")
    public decimal OriginalAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public string Currency { get; set; } = "USD";
    
    // Purchaser Information
    public string PurchasedByUserId { get; set; } = string.Empty;
    public ApplicationUser PurchasedByUser { get; set; } = null!;
    public decimal PurchasePrice { get; set; }
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    
    // Recipient Information
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string? RecipientPhone { get; set; }
    public string? PersonalMessage { get; set; }
    
    // Redemption Information
    public bool IsRedeemed { get; set; } = false;
    public string? RedeemedByUserId { get; set; }
    public ApplicationUser? RedeemedByUser { get; set; }
    public DateTime? RedeemedAt { get; set; }
    
    // Status and Expiration
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    public string Status { get; set; } = "Active"; // Active, PartiallyUsed, FullyRedeemed, Expired, Cancelled
    
    // Payment Information
    public string? StripePaymentIntentId { get; set; }
    public string? StripeRefundId { get; set; }
    
    // Delivery Information
    public bool EmailSent { get; set; } = false;
    public DateTime? EmailSentAt { get; set; }
    public DateTime? ScheduledDeliveryDate { get; set; } // For scheduled future delivery
    
    // Location Restrictions (optional)
    public int? RestrictedToLocationId { get; set; }
    public Location? RestrictedToLocation { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? Notes { get; set; } // Admin notes
    
    // Navigation Properties
    public ICollection<GiftCertificateTransaction> Transactions { get; set; } = new List<GiftCertificateTransaction>();
}

