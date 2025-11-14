namespace SpaBooker.Core.Entities;

/// <summary>
/// Represents which services can be performed in which rooms.
/// This is a many-to-many relationship between Rooms and Services.
/// </summary>
public class RoomServiceCapability
{
    public int Id { get; set; }
    
    // Room Reference
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
    
    // Service Reference
    public int ServiceId { get; set; }
    public SpaService Service { get; set; } = null!;
    
    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

