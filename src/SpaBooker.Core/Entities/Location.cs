namespace SpaBooker.Core.Entities;

public class Location
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<ApplicationUser> Therapists { get; set; } = new List<ApplicationUser>();
    public ICollection<SpaService> Services { get; set; } = new List<SpaService>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}

