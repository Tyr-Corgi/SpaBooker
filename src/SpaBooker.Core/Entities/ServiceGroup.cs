namespace SpaBooker.Core.Entities;

public class ServiceGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;
    public bool RequiresMembership { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<ServiceDuration> Durations { get; set; } = new List<ServiceDuration>();
    public ICollection<ServiceGroupTherapist> Therapists { get; set; } = new List<ServiceGroupTherapist>();
    public ICollection<ServiceGroupRoom> Rooms { get; set; } = new List<ServiceGroupRoom>();
}

