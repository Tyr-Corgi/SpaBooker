namespace SpaBooker.Core.Entities;

public class AuditLog
{
    public int Id { get; set; }
    /// <summary>
    /// The user who performed the action (null for system actions)
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// The user who performed the action
    /// </summary>
    public ApplicationUser? User { get; set; }
    
    /// <summary>
    /// The action performed (e.g., "Login", "Create", "Update", "Delete", "PaymentProcessed")
    /// </summary>
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// The type of entity affected (e.g., "Booking", "User", "Payment")
    /// </summary>
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// The ID of the affected entity (if applicable)
    /// </summary>
    public string? EntityId { get; set; }
    
    /// <summary>
    /// JSON representation of the old value (for updates/deletes)
    /// </summary>
    public string? OldValue { get; set; }
    
    /// <summary>
    /// JSON representation of the new value (for creates/updates)
    /// </summary>
    public string? NewValue { get; set; }
    
    /// <summary>
    /// IP address of the user performing the action
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// User agent string from the request
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// When the action occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Severity level (Info, Warning, Critical)
    /// </summary>
    public string Severity { get; set; } = "Info";
    
    /// <summary>
    /// Additional context or notes about the action
    /// </summary>
    public string? Notes { get; set; }
}

