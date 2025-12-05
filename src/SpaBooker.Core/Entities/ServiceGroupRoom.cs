namespace SpaBooker.Core.Entities;

public class ServiceGroupRoom
{
    public int ServiceGroupId { get; set; }
    public ServiceGroup ServiceGroup { get; set; } = null!;
    
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
    
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
