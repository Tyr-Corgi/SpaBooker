namespace SpaBooker.Core.Entities;

public class InventoryTransaction
{
    public int Id { get; set; }
    public int InventoryItemId { get; set; }
    public InventoryItem InventoryItem { get; set; } = null!;
    
    public int Quantity { get; set; }
    public string Type { get; set; } = string.Empty; // "Purchase", "Use", "Adjustment", "Waste"
    public string? Notes { get; set; }
    public int? RelatedBookingId { get; set; }
    public string? PerformedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

