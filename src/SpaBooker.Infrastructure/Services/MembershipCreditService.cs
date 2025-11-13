using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Enums;
using SpaBooker.Core.Interfaces;
using SpaBooker.Core.Settings;
using SpaBooker.Infrastructure.Data;

namespace SpaBooker.Infrastructure.Services;

public class MembershipCreditService : IMembershipCreditService
{
    private readonly ApplicationDbContext _context;
    private readonly MembershipSettings _settings;

    public MembershipCreditService(
        ApplicationDbContext context,
        IOptions<MembershipSettings> settings)
    {
        _context = context;
        _settings = settings.Value;
    }

    public async Task AddMonthlyCreditsAsync(int userMembershipId, decimal amount, string description)
    {
        var membership = await _context.UserMemberships.FindAsync(userMembershipId);
        if (membership == null) return;

        // Add credits
        membership.CurrentCredits += amount;
        membership.UpdatedAt = DateTime.UtcNow;

        // Log transaction with expiration date
        var transaction = new MembershipCreditTransaction
        {
            UserMembershipId = userMembershipId,
            Amount = amount,
            Type = "Credit",
            Description = description,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMonths(_settings.CreditExpirationMonths)
        };

        _context.MembershipCreditTransactions.Add(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task DeductCreditsAsync(int userMembershipId, decimal amount, string description, int? relatedBookingId = null)
    {
        var membership = await _context.UserMemberships.FindAsync(userMembershipId);
        if (membership == null) return;

        if (membership.CurrentCredits < amount)
        {
            throw new InvalidOperationException($"Insufficient credits. Available: {membership.CurrentCredits}, Required: {amount}");
        }

        // Deduct credits (FIFO - oldest credits used first)
        membership.CurrentCredits -= amount;
        membership.UpdatedAt = DateTime.UtcNow;

        // Log transaction
        var transaction = new MembershipCreditTransaction
        {
            UserMembershipId = userMembershipId,
            Amount = -amount, // Negative for deduction
            Type = "Debit",
            Description = description,
            RelatedBookingId = relatedBookingId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = null // Deductions don't expire
        };

        _context.MembershipCreditTransactions.Add(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetAvailableCreditsAsync(int userMembershipId)
    {
        // First expire any old credits
        await ExpireOldCreditsAsync(userMembershipId);

        var membership = await _context.UserMemberships.FindAsync(userMembershipId);
        return membership?.CurrentCredits ?? 0;
    }

    public async Task ExpireOldCreditsAsync(int userMembershipId)
    {
        var now = DateTime.UtcNow;

        // Find credits that have expired but haven't been marked as expired
        var expiredCredits = await _context.MembershipCreditTransactions
            .Where(t => t.UserMembershipId == userMembershipId
                     && t.Type == "Credit"
                     && t.ExpiresAt.HasValue
                     && t.ExpiresAt.Value <= now)
            .ToListAsync();

        if (!expiredCredits.Any()) return;

        var membership = await _context.UserMemberships.FindAsync(userMembershipId);
        if (membership == null) return;

        // Calculate total expired amount
        var totalExpired = expiredCredits.Sum(c => c.Amount);

        // Deduct expired credits from current balance
        membership.CurrentCredits = Math.Max(0, membership.CurrentCredits - totalExpired);
        membership.UpdatedAt = DateTime.UtcNow;

        // Log expiration transactions
        foreach (var credit in expiredCredits)
        {
            var expirationTransaction = new MembershipCreditTransaction
            {
                UserMembershipId = userMembershipId,
                Amount = -credit.Amount,
                Type = "Expired",
                Description = $"Expired credits from {credit.CreatedAt:MMM yyyy}",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = null
            };

            _context.MembershipCreditTransactions.Add(expirationTransaction);

            // Mark original credit as processed (by updating type)
            credit.Type = "Expired";
        }

        await _context.SaveChangesAsync();
    }

    public async Task ExpireAllOldCreditsAsync()
    {
        var now = DateTime.UtcNow;

        // Find all active memberships
        var activeMemberships = await _context.UserMemberships
            .Where(m => m.Status == MembershipStatus.Active)
            .Select(m => m.Id)
            .ToListAsync();

        foreach (var membershipId in activeMemberships)
        {
            await ExpireOldCreditsAsync(membershipId);
        }
    }
}

