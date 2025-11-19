using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Interfaces;
using SpaBooker.Infrastructure.Data;
using System.Security.Claims;
using System.Text.Json;

namespace SpaBooker.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(string action, string entityType, string? entityId = null,
        string? oldValue = null, string? newValue = null, string? notes = null,
        string severity = "Info")
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userId = httpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();

            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValue = oldValue,
                NewValue = newValue,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow,
                Severity = severity,
                Notes = notes
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Audit log created: {Action} on {EntityType} {EntityId} by user {UserId} from {IpAddress}",
                action, entityType, entityId ?? "N/A", userId ?? "Anonymous", ipAddress);
        }
        catch (Exception ex)
        {
            // Never let audit logging failures break the application
            _logger.LogError(ex, "Failed to create audit log for action {Action} on {EntityType}", action, entityType);
        }
    }

    public async Task LogLoginAttemptAsync(string username, bool success, string? failureReason = null)
    {
        var action = success ? "LoginSuccess" : "LoginFailed";
        var severity = success ? "Info" : "Warning";
        var notes = success ? null : $"Failure reason: {failureReason}";

        await LogAsync(action, "Authentication", username, null, null, notes, severity);
    }

    public async Task LogUserCreatedAsync(string userId, string email)
    {
        var userData = new { UserId = userId, Email = email };
        var newValue = JsonSerializer.Serialize(userData);

        await LogAsync("UserCreated", "User", userId, null, newValue, $"User {email} created", "Info");
    }

    public async Task LogUserModifiedAsync(string userId, string? oldValue, string? newValue)
    {
        await LogAsync("UserModified", "User", userId, oldValue, newValue, null, "Info");
    }

    public async Task LogUserDeletedAsync(string userId, string email)
    {
        var userData = new { UserId = userId, Email = email };
        var oldValue = JsonSerializer.Serialize(userData);

        await LogAsync("UserDeleted", "User", userId, oldValue, null, $"User {email} deleted", "Warning");
    }

    public async Task LogPaymentAsync(string paymentIntentId, decimal amount, bool success, string? notes = null)
    {
        var action = success ? "PaymentSuccess" : "PaymentFailed";
        var severity = success ? "Info" : "Warning";
        var paymentData = new { PaymentIntentId = paymentIntentId, Amount = amount };
        var value = JsonSerializer.Serialize(paymentData);

        await LogAsync(action, "Payment", paymentIntentId, null, value, notes, severity);
    }

    public async Task LogBookingAsync(string action, int bookingId, string? notes = null)
    {
        var severity = action.Contains("Cancel", StringComparison.OrdinalIgnoreCase) ? "Warning" : "Info";
        await LogAsync(action, "Booking", bookingId.ToString(), null, null, notes, severity);
    }

    public async Task LogGiftCertificateAsync(string action, int giftCertificateId, string? notes = null)
    {
        await LogAsync(action, "GiftCertificate", giftCertificateId.ToString(), null, null, notes, "Info");
    }

    public async Task LogAdminActionAsync(string action, string entityType, string? entityId, string? notes = null)
    {
        await LogAsync(action, entityType, entityId, null, null, notes, "Critical");
    }
}

