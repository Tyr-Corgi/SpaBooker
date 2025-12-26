using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Interfaces;
using SpaBooker.Infrastructure.Data;

namespace SpaBooker.Infrastructure.Services;

public class GiftCertificateService : IGiftCertificateService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<GiftCertificateService> _logger;

    public GiftCertificateService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<GiftCertificateService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<GiftCertificate> CreateGiftCertificateAsync(string purchasedByUserId, decimal amount, 
        string recipientName, string recipientEmail, string? recipientPhone = null, 
        string? personalMessage = null, DateTime? scheduledDeliveryDate = null, 
        int? restrictedToLocationId = null)
    {
        var code = await GenerateUniqueCodeAsync();
        var expiresAt = DateTime.UtcNow.AddYears(1); // Gift certificates expire in 1 year

        var giftCertificate = new GiftCertificate
        {
            Code = code,
            OriginalAmount = amount,
            RemainingBalance = amount,
            PurchasedByUserId = purchasedByUserId,
            PurchasePrice = amount,
            PurchasedAt = DateTime.UtcNow,
            RecipientName = recipientName,
            RecipientEmail = recipientEmail,
            RecipientPhone = recipientPhone,
            PersonalMessage = personalMessage,
            ScheduledDeliveryDate = scheduledDeliveryDate,
            RestrictedToLocationId = restrictedToLocationId,
            ExpiresAt = expiresAt,
            Status = "Pending", // Pending until payment is confirmed
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.GiftCertificates.Add(giftCertificate);
        await _context.SaveChangesAsync();

        // Create initial transaction
        var transaction = new GiftCertificateTransaction
        {
            GiftCertificateId = giftCertificate.Id,
            Type = "Purchase",
            Amount = amount,
            BalanceBefore = 0,
            BalanceAfter = amount,
            Description = $"Gift certificate purchased by {purchasedByUserId}",
            PerformedByUserId = purchasedByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.GiftCertificateTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return giftCertificate;
    }

    public async Task<string> ProcessGiftCertificatePurchaseAsync(int giftCertificateId, string stripePaymentIntentId)
    {
        var giftCert = await _context.GiftCertificates.FindAsync(giftCertificateId);
        if (giftCert == null)
        {
            throw new InvalidOperationException("Gift certificate not found");
        }

        giftCert.StripePaymentIntentId = stripePaymentIntentId;
        giftCert.Status = "Active";
        giftCert.UpdatedAt = DateTime.UtcNow;
        
        // FIX: Explicitly mark the entity as modified to ensure EF Core saves all changes
        _context.Entry(giftCert).State = EntityState.Modified;

        await _context.SaveChangesAsync();

        // Send email if not scheduled for later
        if (!giftCert.ScheduledDeliveryDate.HasValue || giftCert.ScheduledDeliveryDate.Value <= DateTime.UtcNow)
        {
            await SendGiftCertificateEmailAsync(giftCertificateId);
        }

        return giftCert.Code;
    }

    public async Task<GiftCertificate?> GetGiftCertificateByCodeAsync(string code)
    {
        return await _context.GiftCertificates
            .Include(gc => gc.PurchasedByUser)
            .Include(gc => gc.RedeemedByUser)
            .Include(gc => gc.RestrictedToLocation)
            .Include(gc => gc.Transactions)
            .FirstOrDefaultAsync(gc => gc.Code == code.ToUpper());
    }

    public async Task<bool> ValidateGiftCertificateAsync(string code)
    {
        var giftCert = await GetGiftCertificateByCodeAsync(code);
        
        if (giftCert == null)
            return false;

        if (!giftCert.IsActive)
            return false;

        if (giftCert.Status != "Active" && giftCert.Status != "PartiallyUsed")
            return false;

        if (giftCert.ExpiresAt.HasValue && giftCert.ExpiresAt.Value < DateTime.UtcNow)
            return false;

        if (giftCert.RemainingBalance <= 0)
            return false;

        return true;
    }

    public async Task<decimal> RedeemGiftCertificateAsync(string code, string redeemedByUserId, decimal amount, int? relatedBookingId = null)
    {
        using var dbTransaction = await _unitOfWork.BeginTransactionAsync();

        try
    {
        var giftCert = await GetGiftCertificateByCodeAsync(code);
        
        if (giftCert == null)
            {
                _logger.LogWarning("Gift certificate redemption failed: code {Code} not found", code);
            throw new InvalidOperationException("Gift certificate not found");
            }

        if (!await ValidateGiftCertificateAsync(code))
            {
                _logger.LogWarning("Gift certificate redemption failed: code {Code} is not valid", code);
            throw new InvalidOperationException("Gift certificate is not valid");
            }

        var amountToRedeem = Math.Min(amount, giftCert.RemainingBalance);

        var balanceBefore = giftCert.RemainingBalance;
        giftCert.RemainingBalance -= amountToRedeem;
        
        if (!giftCert.IsRedeemed && amountToRedeem > 0)
        {
            giftCert.IsRedeemed = true;
            giftCert.RedeemedByUserId = redeemedByUserId;
            giftCert.RedeemedAt = DateTime.UtcNow;
        }

        if (giftCert.RemainingBalance <= 0)
        {
            giftCert.Status = "FullyRedeemed";
        }
        else if (giftCert.RemainingBalance < giftCert.OriginalAmount)
        {
            giftCert.Status = "PartiallyUsed";
        }

        giftCert.UpdatedAt = DateTime.UtcNow;

        // Create transaction
        var transaction = new GiftCertificateTransaction
        {
            GiftCertificateId = giftCert.Id,
            Type = "Redemption",
            Amount = -amountToRedeem,
            BalanceBefore = balanceBefore,
            BalanceAfter = giftCert.RemainingBalance,
            Description = $"Redeemed by user {redeemedByUserId}",
            RelatedBookingId = relatedBookingId,
            PerformedByUserId = redeemedByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.GiftCertificateTransactions.Add(transaction);

            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Gift certificate {Code} redeemed: {Amount} by user {UserId}. New balance: {Balance}",
                code, amountToRedeem, redeemedByUserId, giftCert.RemainingBalance);

        return amountToRedeem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to redeem gift certificate {Code}", code);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<List<GiftCertificate>> GetPurchasedGiftCertificatesAsync(string userId)
    {
        return await _context.GiftCertificates
            .Include(gc => gc.RestrictedToLocation)
            .Include(gc => gc.Transactions)
            .Where(gc => gc.PurchasedByUserId == userId)
            .OrderByDescending(gc => gc.PurchasedAt)
            .ToListAsync();
    }

    public async Task<List<GiftCertificate>> GetReceivedGiftCertificatesAsync(string userEmail)
    {
        return await _context.GiftCertificates
            .Include(gc => gc.PurchasedByUser)
            .Include(gc => gc.RestrictedToLocation)
            .Include(gc => gc.Transactions)
            .Where(gc => gc.RecipientEmail.ToLower() == userEmail.ToLower() && gc.Status == "Active" || gc.Status == "PartiallyUsed")
            .OrderByDescending(gc => gc.PurchasedAt)
            .ToListAsync();
    }

    public async Task<GiftCertificate?> GetGiftCertificateByIdAsync(int id)
    {
        return await _context.GiftCertificates
            .Include(gc => gc.PurchasedByUser)
            .Include(gc => gc.RedeemedByUser)
            .Include(gc => gc.RestrictedToLocation)
            .Include(gc => gc.Transactions)
                .ThenInclude(t => t.RelatedBooking)
            .FirstOrDefaultAsync(gc => gc.Id == id);
    }

    public async Task<List<GiftCertificate>> GetAllGiftCertificatesAsync(string? status = null, int? locationId = null)
    {
        var query = _context.GiftCertificates
            .Include(gc => gc.PurchasedByUser)
            .Include(gc => gc.RedeemedByUser)
            .Include(gc => gc.RestrictedToLocation)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(gc => gc.Status == status);
        }

        if (locationId.HasValue)
        {
            query = query.Where(gc => gc.RestrictedToLocationId == locationId);
        }

        return await query
            .OrderByDescending(gc => gc.PurchasedAt)
            .ToListAsync();
    }

    public async Task CancelGiftCertificateAsync(int id, string reason)
    {
        var giftCert = await _context.GiftCertificates.FindAsync(id);
        if (giftCert == null)
            throw new InvalidOperationException("Gift certificate not found");

        giftCert.Status = "Cancelled";
        giftCert.IsActive = false;
        giftCert.Notes = reason;
        giftCert.UpdatedAt = DateTime.UtcNow;
        
        // FIX: Explicitly mark the entity as modified to ensure EF Core saves all changes
        _context.Entry(giftCert).State = EntityState.Modified;

        await _context.SaveChangesAsync();
    }

    public async Task RefundGiftCertificateAsync(int id, string stripeRefundId)
    {
        var giftCert = await _context.GiftCertificates.FindAsync(id);
        if (giftCert == null)
            throw new InvalidOperationException("Gift certificate not found");

        giftCert.StripeRefundId = stripeRefundId;
        giftCert.Status = "Refunded";
        giftCert.IsActive = false;
        giftCert.UpdatedAt = DateTime.UtcNow;
        
        // FIX: Explicitly mark the entity as modified to ensure EF Core saves all changes
        _context.Entry(giftCert).State = EntityState.Modified;

        // Create refund transaction
        var transaction = new GiftCertificateTransaction
        {
            GiftCertificateId = giftCert.Id,
            Type = "Refund",
            Amount = -giftCert.RemainingBalance,
            BalanceBefore = giftCert.RemainingBalance,
            BalanceAfter = 0,
            Description = $"Gift certificate refunded: {stripeRefundId}",
            CreatedAt = DateTime.UtcNow
        };

        giftCert.RemainingBalance = 0;

        _context.GiftCertificateTransactions.Add(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<string> GenerateUniqueCodeAsync()
    {
        string code;
        bool exists;

        do
        {
            // Generate format: GIFT-XXXX-XXXX
            var random = new Random();
            var part1 = random.Next(1000, 9999).ToString();
            var part2 = random.Next(1000, 9999).ToString();
            code = $"GIFT-{part1}-{part2}";

            exists = await _context.GiftCertificates.AnyAsync(gc => gc.Code == code);
        }
        while (exists);

        return code;
    }

    public async Task ProcessExpiredGiftCertificatesAsync()
    {
        var expiredCerts = await _context.GiftCertificates
            .Where(gc => gc.IsActive 
                      && gc.ExpiresAt.HasValue 
                      && gc.ExpiresAt.Value < DateTime.UtcNow
                      && gc.Status != "Expired")
            .ToListAsync();

        foreach (var cert in expiredCerts)
        {
            cert.Status = "Expired";
            cert.IsActive = false;
            cert.UpdatedAt = DateTime.UtcNow;

            // Create expiration transaction
            var transaction = new GiftCertificateTransaction
            {
                GiftCertificateId = cert.Id,
                Type = "Expiration",
                Amount = -cert.RemainingBalance,
                BalanceBefore = cert.RemainingBalance,
                BalanceAfter = 0,
                Description = $"Gift certificate expired on {cert.ExpiresAt!.Value:yyyy-MM-dd}",
                CreatedAt = DateTime.UtcNow
            };

            cert.RemainingBalance = 0;
            _context.GiftCertificateTransactions.Add(transaction);
        }

        await _context.SaveChangesAsync();
    }

    public async Task SendGiftCertificateEmailAsync(int giftCertificateId)
    {
        var giftCert = await GetGiftCertificateByIdAsync(giftCertificateId);
        if (giftCert == null)
            throw new InvalidOperationException("Gift certificate not found");

        if (giftCert.EmailSent)
            return; // Already sent

        // Generate email HTML
        var htmlMessage = GenerateGiftCertificateEmail(giftCert);
        
        try
        {
            await _emailService.SendEmailAsync(
                giftCert.RecipientEmail,
                $"🎁 You've Received a SpaBooker Gift Certificate!",
                htmlMessage
            );

            giftCert.EmailSent = true;
            giftCert.EmailSentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error but don't fail
            Console.WriteLine($"Failed to send gift certificate email: {ex.Message}");
        }
    }

    private string GenerateGiftCertificateEmail(GiftCertificate giftCert)
    {
        var locationInfo = giftCert.RestrictedToLocation != null 
            ? $"<p><strong>Valid at:</strong> {giftCert.RestrictedToLocation.Name}</p>"
            : "<p><strong>Valid at:</strong> All SpaBooker locations</p>";

        var messageSection = !string.IsNullOrEmpty(giftCert.PersonalMessage)
            ? $@"
                <div style='background-color: #fff5f5; padding: 15px; border-left: 4px solid #b76e79; margin: 20px 0;'>
                    <p style='margin: 0; font-style: italic;'>&ldquo;{giftCert.PersonalMessage}&rdquo;</p>
                    <p style='margin: 5px 0 0 0; text-align: right; font-size: 0.9em;'>- {giftCert.PurchasedByUser.FirstName} {giftCert.PurchasedByUser.LastName}</p>
                </div>"
            : "";

        return $@"
        <div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: auto; border: 1px solid #f0e0e0; border-radius: 8px; overflow: hidden;'>
            <div style='background: linear-gradient(135deg, #b76e79 0%, #e8b4b8 100%); padding: 30px; text-align: center;'>
                <h1 style='color: white; margin: 0; font-size: 2em;'>🎁 Gift Certificate 🎁</h1>
            </div>
            <div style='padding: 30px;'>
                <p style='font-size: 1.2em;'>Dear {giftCert.RecipientName},</p>
                <p>You've received a special gift from {giftCert.PurchasedByUser.FirstName} {giftCert.PurchasedByUser.LastName}!</p>
                
                {messageSection}
                
                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; text-align: center;'>
                    <h2 style='color: #b76e79; margin: 0 0 10px 0;'>${giftCert.OriginalAmount:F2}</h2>
                    <p style='font-size: 1.5em; font-weight: bold; color: #333; margin: 10px 0; letter-spacing: 2px; font-family: monospace;'>{giftCert.Code}</p>
                    <p style='margin: 10px 0 0 0; font-size: 0.9em; color: #666;'>Your Redemption Code</p>
                </div>
                
                <h3 style='color: #b76e79;'>How to Redeem:</h3>
                <ol style='line-height: 1.8;'>
                    <li>Visit SpaBooker and select your desired service</li>
                    <li>During checkout, enter your gift certificate code</li>
                    <li>The amount will be applied to your booking</li>
                </ol>
                
                <h3 style='color: #b76e79;'>Gift Certificate Details:</h3>
                <ul style='list-style: none; padding: 0;'>
                    <li><strong>Amount:</strong> ${giftCert.OriginalAmount:F2}</li>
                    <li><strong>Code:</strong> {giftCert.Code}</li>
                    <li><strong>Expires:</strong> {giftCert.ExpiresAt?.ToString("MMMM dd, yyyy") ?? "No expiration"}</li>
                    {(giftCert.RestrictedToLocationId.HasValue ? $"<li><strong>Location:</strong> {giftCert.RestrictedToLocation!.Name}</li>" : "")}
                </ul>
                
                <div style='background-color: #fff9e6; padding: 15px; border-radius: 4px; margin-top: 20px;'>
                    <p style='margin: 0; font-size: 0.9em;'><strong>💡 Tip:</strong> Save this email! You'll need your gift certificate code to redeem your spa experience.</p>
                </div>
                
                <div style='text-align: center; margin-top: 30px;'>
                    <a href='https://spabooker.com/services' style='background: linear-gradient(135deg, #b76e79 0%, #e8b4b8 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 25px; display: inline-block; font-weight: bold;'>Book Your Spa Experience</a>
                </div>
                
                <p style='margin-top: 30px; font-size: 0.9em; color: #666;'>Questions? Contact us at <a href='mailto:support@spabooker.com' style='color: #b76e79;'>support@spabooker.com</a></p>
            </div>
            <div style='background-color: #f8d7da; padding: 15px; text-align: center; font-size: 0.8em; color: #b76e79;'>
                <p>&copy; {DateTime.Now.Year} SpaBooker. All rights reserved.</p>
            </div>
        </div>";
    }
}

