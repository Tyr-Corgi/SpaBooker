namespace SpaBooker.Core.Entities;

/// <summary>
/// Junction table for many-to-many relationship between Services and Therapists
/// </summary>
public class ServiceTherapist
{
    public int ServiceId { get; set; }
    public SpaService Service { get; set; } = null!;
    
    public string TherapistId { get; set; } = string.Empty;
    public ApplicationUser Therapist { get; set; } = null!;
    
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}

