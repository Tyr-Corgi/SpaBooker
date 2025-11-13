namespace SpaBooker.Core.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody, string? plainTextBody = null);
    Task SendBookingConfirmationAsync(int bookingId);
    Task SendBookingCancellationAsync(int bookingId);
    Task SendBookingReminderAsync(int bookingId);
    Task SendMembershipConfirmationAsync(int userMembershipId);
    Task SendMembershipCancellationAsync(int userMembershipId);
}

