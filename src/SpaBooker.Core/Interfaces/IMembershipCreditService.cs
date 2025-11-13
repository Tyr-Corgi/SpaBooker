namespace SpaBooker.Core.Interfaces;

public interface IMembershipCreditService
{
    Task AddMonthlyCreditsAsync(int userMembershipId, decimal amount, string description);
    Task DeductCreditsAsync(int userMembershipId, decimal amount, string description, int? relatedBookingId = null);
    Task<decimal> GetAvailableCreditsAsync(int userMembershipId);
    Task ExpireOldCreditsAsync(int userMembershipId);
    Task ExpireAllOldCreditsAsync(); // Background job to run daily
}

