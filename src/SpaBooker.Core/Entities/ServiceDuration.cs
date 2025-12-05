namespace SpaBooker.Core.Entities;

public class ServiceDuration
{
    public int Id { get; set; }
    public int ServiceGroupId { get; set; }
    public ServiceGroup ServiceGroup { get; set; } = null!;
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

