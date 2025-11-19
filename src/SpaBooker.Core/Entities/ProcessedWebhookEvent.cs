namespace SpaBooker.Core.Entities;

/// <summary>
/// Tracks processed webhook events to prevent duplicate processing (idempotency)
/// </summary>
public class ProcessedWebhookEvent
{
    public int Id { get; set; }
    public string StripeEventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

