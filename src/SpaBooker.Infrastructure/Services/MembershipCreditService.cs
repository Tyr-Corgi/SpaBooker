using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MembershipCreditService> _logger;
    private readonly MembershipSettings _settings;

    public MembershipCreditService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<MembershipCreditService> logger,
        IOptions<MembershipSettings> settings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task AddMonthlyCreditsAsync(int userMembershipId, decimal amount, string description)
    {
        using var dbTransaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var membership = await _context.UserMemberships.FindAsync(userMembershipId);
            if (membership == null)
            {
                _logger.LogWarning("Attempted to add credits to non-existent membership {MembershipId}", userMembershipId);
                return;
            }

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

            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Added {Amount} credits to membership {MembershipId}. New balance: {Balance}",
                amount, userMembershipId, membership.CurrentCredits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add credits to membership {MembershipId}", userMembershipId);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task DeductCreditsAsync(int userMembershipId, decimal amount, string description, int? relatedBookingId = null)
    {
        using var dbTransaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var membership = await _context.UserMemberships.FindAsync(userMembershipId);
            if (membership == null)
            {
                _logger.LogWarning("Attempted to deduct credits from non-existent membership {MembershipId}", userMembershipId);
                return;
            }

            if (membership.CurrentCredits < amount)
            {
                var insufficientCreditsMessage = $"Insufficient credits. Available: {membership.CurrentCredits}, Required: {amount}";
                _logger.LogWarning("Credit deduction failed for membership {MembershipId}: {Message}",
                    userMembershipId, insufficientCreditsMessage);
                throw new InvalidOperationException(insufficientCreditsMessage);
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

            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Deducted {Amount} credits from membership {MembershipId}. New balance: {Balance}",
                amount, userMembershipId, membership.CurrentCredits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deduct credits from membership {MembershipId}", userMembershipId);
            await _unitOfWork.RollbackAsync();
            throw;
        }
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

        using var dbTransaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var membership = await _context.UserMemberships.FindAsync(userMembershipId);
            if (membership == null)
            {
                _logger.LogWarning("Attempted to expire credits for non-existent membership {MembershipId}", userMembershipId);
                return;
            }

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

            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Expired {Amount} credits for membership {MembershipId}. New balance: {Balance}",
                totalExpired, userMembershipId, membership.CurrentCredits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to expire credits for membership {MembershipId}", userMembershipId);
            await _unitOfWork.RollbackAsync();
            throw;
        }
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

