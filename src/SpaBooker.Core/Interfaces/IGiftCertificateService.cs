using SpaBooker.Core.Entities;

namespace SpaBooker.Core.Interfaces;

public interface IGiftCertificateService
{
    // Purchase
    Task<GiftCertificate> CreateGiftCertificateAsync(string purchasedByUserId, decimal amount, 
        string recipientName, string recipientEmail, string? recipientPhone = null, 
        string? personalMessage = null, DateTime? scheduledDeliveryDate = null, 
        int? restrictedToLocationId = null);
    
    Task<string> ProcessGiftCertificatePurchaseAsync(int giftCertificateId, string stripePaymentIntentId);
    
    // Redemption
    Task<GiftCertificate?> GetGiftCertificateByCodeAsync(string code);
    Task<bool> ValidateGiftCertificateAsync(string code);
    Task<decimal> RedeemGiftCertificateAsync(string code, string redeemedByUserId, decimal amount, int? relatedBookingId = null);
    
    // Query
    Task<List<GiftCertificate>> GetPurchasedGiftCertificatesAsync(string userId);
    Task<List<GiftCertificate>> GetReceivedGiftCertificatesAsync(string userEmail);
    Task<GiftCertificate?> GetGiftCertificateByIdAsync(int id);
    
    // Admin
    Task<List<GiftCertificate>> GetAllGiftCertificatesAsync(string? status = null, int? locationId = null);
    Task CancelGiftCertificateAsync(int id, string reason);
    Task RefundGiftCertificateAsync(int id, string stripeRefundId);
    
    // Utilities
    Task<string> GenerateUniqueCodeAsync();
    Task ProcessExpiredGiftCertificatesAsync();
    Task SendGiftCertificateEmailAsync(int giftCertificateId);
}

