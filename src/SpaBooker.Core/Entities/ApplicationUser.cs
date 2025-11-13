using Microsoft.AspNetCore.Identity;

namespace SpaBooker.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    
    // For Therapists
    public int? LocationId { get; set; }
    public Location? Location { get; set; }
    public string? Bio { get; set; }
    public string? Specialties { get; set; }
    
    // Navigation properties
    public ICollection<Booking> BookingsAsClient { get; set; } = new List<Booking>();
    public ICollection<Booking> BookingsAsTherapist { get; set; } = new List<Booking>();
    public ICollection<UserMembership> Memberships { get; set; } = new List<UserMembership>();
    public ICollection<TherapistAvailability> Availability { get; set; } = new List<TherapistAvailability>();
}

