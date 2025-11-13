namespace SpaBooker.Core.Entities;

public class SpaService
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal BasePrice { get; set; }
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public bool RequiresMembership { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<ServiceTherapist> ServiceTherapists { get; set; } = new List<ServiceTherapist>();
}

