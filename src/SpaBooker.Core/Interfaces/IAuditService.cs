namespace SpaBooker.Core.Interfaces;

public interface IAuditService
{
    /// <summary>
    /// Log a security-relevant event
    /// </summary>
    Task LogAsync(string action, string entityType, string? entityId = null, 
        string? oldValue = null, string? newValue = null, string? notes = null, 
        string severity = "Info");
    
    /// <summary>
    /// Log a login attempt
    /// </summary>
    Task LogLoginAttemptAsync(string username, bool success, string? failureReason = null);
    
    /// <summary>
    /// Log a user creation
    /// </summary>
    Task LogUserCreatedAsync(string userId, string email);
    
    /// <summary>
    /// Log a user modification
    /// </summary>
    Task LogUserModifiedAsync(string userId, string? oldValue, string? newValue);
    
    /// <summary>
    /// Log a user deletion
    /// </summary>
    Task LogUserDeletedAsync(string userId, string email);
    
    /// <summary>
    /// Log a payment transaction
    /// </summary>
    Task LogPaymentAsync(string paymentIntentId, decimal amount, bool success, string? notes = null);
    
    /// <summary>
    /// Log a booking action
    /// </summary>
    Task LogBookingAsync(string action, int bookingId, string? notes = null);
    
    /// <summary>
    /// Log a gift certificate action
    /// </summary>
    Task LogGiftCertificateAsync(string action, int giftCertificateId, string? notes = null);
    
    /// <summary>
    /// Log an administrative action
    /// </summary>
    Task LogAdminActionAsync(string action, string entityType, string? entityId, string? notes = null);
}

