namespace SpaBooker.Core.Entities;

public class InventoryItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;
    
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public string Unit { get; set; } = "unit"; // unit, oz, ml, etc.
    
    public decimal? CostPerUnit { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
}

