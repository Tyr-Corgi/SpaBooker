namespace SpaBooker.Core.Entities;

public class Room
{
    public int Id { get; set; }
    
    // Room Details
    public string Name { get; set; } = string.Empty; // "Room 1", "Suite A", "Massage Room 2"
    public string? Description { get; set; }
    
    // Location Reference
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;
    
    // Display Settings
    public bool IsActive { get; set; } = true;
    public string ColorCode { get; set; } = "#007bff"; // Hex color for UI display
    public int DisplayOrder { get; set; } = 1; // For sorting in UI
    
    // Audit Fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public ICollection<RoomServiceCapability> ServiceCapabilities { get; set; } = new List<RoomServiceCapability>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

