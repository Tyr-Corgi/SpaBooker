using SpaBooker.Core.Enums;

namespace SpaBooker.Core.Entities;

public class Booking
{
    public int Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public ApplicationUser Client { get; set; } = null!;
    
    public string TherapistId { get; set; } = string.Empty;
    public ApplicationUser Therapist { get; set; } = null!;
    
    public int ServiceId { get; set; }
    public SpaService Service { get; set; } = null!;
    
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    
    public decimal ServicePrice { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal? DiscountApplied { get; set; }
    public decimal TotalPrice { get; set; }
    
    public bool UsedMembershipCredits { get; set; }
    public decimal? CreditsUsed { get; set; }
    
    public string? StripePaymentIntentId { get; set; }
    public string? StripeRefundId { get; set; }
    
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Concurrency token for preventing double-booking
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

